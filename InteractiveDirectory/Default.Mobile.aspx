<%@ Page Title="Hajoca Mobile Directory" Language="C#" MasterPageFile="~/Site.Mobile.Master" AutoEventWireup="true" CodeBehind="Default.Mobile.aspx.cs" Inherits="InteractiveDirectory.Default_Mobile" %>

<%@ Register Src="~/ViewSwitcher.ascx" TagPrefix="friendlyUrls" TagName="ViewSwitcher" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <input class="form-control" id="filter" placeholder="Filter" data-ng-model="filter">
    <div data-role="page" id="list" data-title="Hajoca Mobile Directory">
        <div data-role="panel" id="menuPanel">
            <fieldset data-role="controlgroup" data-mini="true">
                <legend>Directory Listing:</legend>
                <input type="radio" name="listing" data-ng-model="listing" id="listingALL" value="people" checked="checked" data-rel="close">
                <label for="listingALL">People</label>
                <input type="radio" name="listing" data-ng-model="listing" id="listingPC" value="iPC" data-rel="close">
                <label for="listingPC">Profit Centers</label>
                <input type="radio" name="listing" data-ng-model="listing" id="listingPM" value="iPM" data-rel="close">
                <label for="listingPM">PC Managers</label>
                <input type="radio" name="listing" data-ng-model="listing" id="listingRM" value="iRM" data-rel="close">
                <label for="listingRM">Region Managers</label>
                <input type="radio" name="listing" data-ng-model="listing" id="listingRS" value="iRS" data-rel="close">
                <label for="listingRS">Region Support</label>
                <input type="radio" name="listing" data-ng-model="listing" id="listingSC" value="iSC" data-rel="close">
                <label for="listingSC">Nation Support Center</label>
            </fieldset>
            <br />
            <fieldset data-role="controlgroup" data-mini="true">
                <legend>View:</legend>
                <input type="radio" name="view" data-ng-model="view" id="viewStandard" value="0" checked="checked" data-rel="close">
                <label for="viewStandard">Standard</label>
                <input type="radio" name="view" data-ng-model="view" id="viewExtended" value="1" data-rel="close">
                <label for="viewExtended">Extended</label>
            </fieldset>
        </div>

        <div data-role="header" data-position="fixed">
            <a href="#menuPanel" data-icon="bars" data-iconpos="notext">Menu</a>
            <h2>{{listingDisplay}}</h2>
        </div>
        <div role="main" class="ui-content" id="waitContent" style="text-align: center">
            <img src="styles/images/ajax-loader-orange-bar.gif" />
        </div>
        <div role="main" class="ui-content" id="mainContent" style="display: none">
            <ul id="listMobile" data-role="listview" data-filter="true" data-autodividers="true">
                <li class="showDetails" data-ng-repeat="phoneEntry in filteredPhoneData">
                    <a href="#details" data-ng-click="setSelectedPhone(phoneEntry)" data-transition="none">{{phoneEntry.N}}
                        <span>{{phoneEntry.iPC | iif : 'PC '+ phoneEntry.PC : phoneEntry.MD}}</span>
                        <div>
                            <div class="mainLabel" style='display: {{view=="1" | iif : "block" : "none"}}'>
                                <div data-ng-click="OpenPhone(phoneEntry.P)" style='display: {{phoneEntry.P.length>0 | iif : "inline-block" : "none"}}'>
                                    <span class="ui-btn ui-mini ui-shadow ui-corner-all ui-icon-phone ui-btn-icon-notext ui-btn-inline">Phone</span>
                                    P: {{phoneEntry.P}}
                                </div>
                                <div data-ng-click="OpenPhone(phoneEntry.M)" style='display: {{phoneEntry.M.length>0 | iif : "inline-block" : "none"}}'>
                                    <span class="ui-btn ui-mini ui-shadow ui-corner-all ui-icon-phone ui-btn-icon-notext ui-btn-inline">Mobile</span>
                                    M: {{phoneEntry.M}}
                                </div>
                                <div data-ng-click="OpenPhone(phoneEntry.EM)" style='display: {{phoneEntry.EM.length>0 | iif : "inline-block" : "none"}}'>
                                    <span class="ui-btn ui-mini ui-shadow ui-corner-all ui-icon-mail ui-btn-icon-notext ui-btn-inline">Mail</span>
                                    {{phoneEntry.EM}}
                                </div>
                            </div>
                        </div>
                    </a>
                </li>
            </ul>
            <div style="position: fixed; bottom: 0; left: 10px; z-index: 9999;">
                <button id="jumpTo_A" class="ui-btn ui-btn-inline jump-button ui-mini ui-btn-b">A-D</button>
                <button id="jumpTo_E" class="ui-btn ui-btn-inline jump-button ui-mini ui-btn-b">E-H</button>
                <button id="jumpTo_J" class="ui-btn ui-btn-inline jump-button ui-mini ui-btn-b">J-M</button>
                <button id="jumpTo_N" class="ui-btn ui-btn-inline jump-button ui-mini ui-btn-b">N-Q</button>
                <button id="jumpTo_R" class="ui-btn ui-btn-inline jump-button ui-mini ui-btn-b">R-T</button>
                <button id="jumpTo_U" class="ui-btn ui-btn-inline jump-button ui-mini ui-btn-b">U-Z</button>
            </div>
        </div>
        <div data-role="footer" style="padding-top: 10px; text-align: center; height: 65px;">
            <friendlyUrls:ViewSwitcher runat="server" />
        </div>

    </div>

    <div data-role="page" id="details">
        <div data-role="header">
            <a href="#" data-rel="back" data-direction="reverse" data-icon="arrow-l" data-iconpos="notext">Back</a>
            <h2>{{selectedPhone.N}}</h2>
        </div>
        <div role="main" class="ui-content">
            <ul data-role="listview">
                <li style='display: {{selectedPhone.PC.length>0 | iif : "block" : "none"}}'><span class="detailLabel">PC</span>
                    <span class="mobilePhoneDetail">{{selectedPhone.PC}}</span></li>
                <li style='display: {{selectedPhone.P.length>0 | iif : "block" : "none"}}'><span class="detailLabel">Phone #</span>
                    <div class="mobilePhoneDetail" data-ng-click="OpenPhone(selectedPhone.P)">
                        {{selectedPhone.P}}
                        <span class="ui-btn ui-shadow ui-corner-all ui-icon-phone ui-btn-icon-notext ui-btn-inline">Phone #</span>
                    </div>
                </li>
                <li style='display: {{selectedPhone.MD.length>0 | iif : "block" : "none"}}'><span class="detailLabel">{{selectedPhone.iPC | iif : "Manager " : "Department"}}</span>
                    <span class="mobilePhoneDetail">{{selectedPhone.MD}}</span></li>
                <li style='display: {{selectedPhone.M.length>0 | iif : "block" : "none"}}'><span class="detailLabel">{{selectedPhone.iPC | iif : "PCM " : ""}} Mobile #</span>
                    <div class="mobilePhoneDetail" data-ng-click="OpenPhone(selectedPhone.M)">
                        {{selectedPhone.M}}
                        <span class="ui-btn ui-shadow ui-corner-all ui-icon-phone ui-btn-icon-notext ui-btn-inline">Mobile</span>
                    </div>
                </li>
                <li style='display: {{selectedPhone.EM.length>0 | iif : "block" : "none"}}'><span class="detailLabel">Email</span>
                    <div class="mobilePhoneDetail" data-ng-click="OpenEmail(selectedPhone.EM)">
                        {{selectedPhone.EM}}
                        <span class="ui-btn ui-shadow ui-corner-all ui-icon-mail ui-btn-icon-notext ui-btn-inline">Email</span>
                    </div>
                </li>
                <li style='display: {{(selectedPhone.S.length+selectedPhone.C.length)>0 | iif : "block" : "none"}}'><span class="detailLabel">Address</span>
                    <div class="mobilePhoneDetail" data-ng-click="OpenMap(selectedPhone.S + ',' + selectedPhone.C)">
                        <span class="ui-btn ui-shadow ui-corner-all ui-icon-navigation ui-btn-icon-notext ui-btn-inline">Map</span>
                    </div>
                    <div class="mobilePhoneDetail" data-ng-click="OpenMap(selectedPhone.S + ',' + selectedPhone.C)">
                        {{selectedPhone.S}}
                        <br />
                        {{selectedPhone.C}}
                    </div>
                </li>
                <li style='display: {{selectedPhone.RN.length>0 | iif : "block" : "none"}}'><span class="detailLabel">Region Name</span>
                    <span class="mobilePhoneDetail">{{selectedPhone.RN}}</span></li>
                <li style='display: {{selectedPhone.R.length>0 | iif : "block" : "none"}}'><span class="detailLabel">Region Number</span>
                    <span class="mobilePhoneDetail">{{selectedPhone.R}}</span></li>
                <li style='display: {{selectedPhone.E.length>0 | iif : "block" : "none"}}'><span class="detailLabel">Eclipse Box</span>
                    <span class="mobilePhoneDetail">{{selectedPhone.E}}</span></li>
                <li style='display: {{selectedPhone.OC.length>0 | iif : "block" : "none"}}'><span class="detailLabel">On-Site Contact</span>
                    <span class="mobilePhoneDetail">{{selectedPhone.OC}}</span></li>
            </ul>
        </div>
    </div>

</asp:Content>
