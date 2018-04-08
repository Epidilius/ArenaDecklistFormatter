<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h1>MTG: Arena Importer</h1>
        <p class="lead">A tool to correctly format your deck lists to be imported into Arena</p>
    </div>

    <div class="row">
        <div class="col-md-6">
            <h2>Your List</h2>
            <p>
                Copy and paste your deck list into the text box
            </p>
            <div>
                <textarea ID="DeckListPreFormat" runat="server"></textarea>
            </div>
            <br />
            <div>
                <asp:Button ID="button_Format" runat="server" Text="Format Deck" OnClick="button_Format_Click" />
            </div>
        </div>
        <div class="col-md-6">
            <h2>Your List, but Formatted</h2>
            <p>
                Copy this list, open Arena, and click &quot;Import&quot;
            </p>
            <div>
                <textarea ID="DeckListPostFormat" runat="server"></textarea>
            </div>
        </div>
    </div>

    <style type="text/css">
        textarea {
            max-width: 1000px;
            width: 500px;
            height: 350px;
        }
    </style>
</asp:Content>
