using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : Page
{
    static string SCRYFALL_QUERY_BASE = @"https://api.scryfall.com/cards/named?fuzzy=";
    List<string> ArenaLegalSets;

    protected void Page_Load(object sender, EventArgs e)
    {
        SetupSetList();
    }

    void SetupSetList()
    {
        ArenaLegalSets = new List<string>();
        ArenaLegalSets.Add("AKH");
        ArenaLegalSets.Add("HOU");
        ArenaLegalSets.Add("XLN");
        ArenaLegalSets.Add("RIX");
    }

    protected void button_Format_Click(object sender, EventArgs e)
    {
        DeckListPostFormat.InnerText = "";
        FormatDeckList();
    }

    bool ListInAppropriateFormat()
    {
        //TODO: ???
        return true;
    }
    void FormatDeckList()
    {
        var deckList = PrepDeckList(DeckListPreFormat.InnerText);

        deckList = RemoveExes(deckList);
        deckList = DealWithSlashes(deckList);
        deckList = DealWithParentheses(deckList);
        deckList = ClumpDuplicates(deckList);

        deckList = AddCardDetails(deckList);

        PopulatePostField(deckList);
    }
    void PopulatePostField(List<string> deckList)
    {
        for (int i = 0; i < deckList.Count; i++)
        {
            if (i > 0)
                DeckListPostFormat.InnerText += Environment.NewLine;
            DeckListPostFormat.InnerText += deckList[i];
        }
    }

    List<string> PrepDeckList(string deckList)
    {
        return Regex.Split(deckList, Environment.NewLine).ToList();
    }

    //REMOVAL FUNCTIONS
    List<string> RemoveExes(List<string> deckList)
    {
        var formattedCards = new List<string>();

        for (int i = 0; i < deckList.Count; i++)
        {
            var card = deckList[i];

            if (String.IsNullOrEmpty(card))
            {
                continue;
            }

            card = card.Trim();
            var splitCard = Regex.Split(card, " ");
            card = "";

            if (!Char.IsLetter(splitCard[0][0]))
            {
                splitCard[0] = Regex.Match(splitCard[0], @"\d+").Value;
            }
            else
            {
                card = "1 ";
            }

            for (int j = 0; j < splitCard.Length; j++)
            {
                card += " " + splitCard[j];
                card = card.Trim();
            }
            formattedCards.Add(card);
        }

        return formattedCards;
    }
    List<string> DealWithSlashes(List<string> deckList)
    {
        var formattedCards = new List<string>();

        for (int i = 0; i < deckList.Count; i++)
        {
            var card = deckList[i];

            if (card.Contains("/"))
            {
                card = Regex.Replace(card, "[/]+", @"///");
            }

            formattedCards.Add(card);
        }

        return formattedCards;
    }
    List<string> DealWithParentheses(List<string> deckList)
    {
        var formattedCards = new List<string>();

        for (int i = 0; i < deckList.Count; i++)
        {
            var card = deckList[i];

            if (card.Contains("("))
            {
                //card = Regex.Split(card, @"(").First();
                card = card.Split('(').First();
                card.Trim();
            }

            formattedCards.Add(card);
        }

        return formattedCards;
    }
    List<string> ClumpDuplicates(List<string> deckList)
    {
        //TODO: Might not need to do this
        return deckList;
    }

    //ADDITION FUNCTIONS
    List<string> AddCardDetails(List<string> deckList)
    {
        List<string> formattedList = new List<string>();
        for (int i = 0; i < deckList.Count; i++)
        {
            var details = FindCardDetails(deckList[i]);
            if (details["name"] == "")
                continue;

            var card = deckList[i].Split(' ').First() + " " + details["name"] + " (" + details["expansion"] + ") " + details["number"];
            formattedList.Add(card);
        }
        return formattedList;
    }

    //DATA FETCHERS
    Dictionary<string, string> FindCardDetails(string cardName)
    {
        Dictionary<string, string> cardDetails = new Dictionary<string, string>();
        cardDetails.Add("name", "");
        cardDetails.Add("expansion", "");
        cardDetails.Add("number", "");

        var splitName = Regex.Split(cardName, " ");
        cardName = "";
        for (int i = 1; i < splitName.Length; i++)
        {
            cardName += splitName[i] + " ";
        }
        cardName = cardName.Trim();

        var details = GetScryfallData(cardName);

        cardDetails["name"]      = details["CardName"];
        cardDetails["expansion"] = details["Expansion"];
        cardDetails["number"]    = details["CardNumber"];

        return cardDetails;
    }
    Dictionary<string, string> GetScryfallData(string cardName)
    {
        Dictionary<string, string> cardDetails = new Dictionary<string, string>();
        cardDetails.Add("CardName", "");
        cardDetails.Add("Expansion", "");
        cardDetails.Add("CardNumber", "");

        var nameWithoutWhitespace = Regex.Replace(cardName, @"\s+", "+");
        var url = SCRYFALL_QUERY_BASE + nameWithoutWhitespace;
        
        try
        {
            var json = GetDataFromURL(url);
                        
            if(DoesSetMatchArenaSet(json["set"].ToString().ToUpper()) == false)
            {
                var furtherSearch = GetDataFromURL(json["prints_search_uri"].ToString());

                foreach(var card in furtherSearch["data"])
                {
                    if(DoesSetMatchArenaSet(card["set"].ToString().ToUpper()) == true)
                    {
                        cardDetails["CardName"]   = card["name"].ToString();
                        cardDetails["Expansion"]  = card["set"].ToString().ToUpper();
                        cardDetails["CardNumber"] = card["collector_number"].ToString();
                        break;
                    }
                }
            }
            else
            {
                cardDetails["CardName"]   = json["name"].ToString();
                cardDetails["Expansion"]  = json["set"].ToString().ToUpper();
                cardDetails["CardNumber"] = json["collector_number"].ToString();
            }
        }
        catch (Exception ex)
        {

        }

        return cardDetails;
    }
    JObject GetDataFromURL(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;

        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var html = reader.ReadToEnd();
                return JObject.Parse(html);
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    bool DoesSetMatchArenaSet(string set)
    {
        for(int i = 0; i < ArenaLegalSets.Count; i++)
        {
            if(ArenaLegalSets[i] == set)
            {
                return true;
            }
        }

        return false;
    }
}