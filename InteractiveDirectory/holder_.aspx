<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Mobile.Master" AutoEventWireup="true" CodeBehind="Default.Mobile.aspx.cs" Inherits="InteractiveDirectory.Default_Mobile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <nav class="col-xs-1" id="alphaScroll">
                <ul class="nav">
                    <li data-ng-repeat="(firstLetter,PhoneData) in filteredPhoneData"><a href="#alpha{{firstLetter}}">{{firstLetter}}</a></li>
                </ul>
            </nav>
            <div class="col-xs-11">
                <input class="form-control" id="filter" placeholder="Filter" data-ng-model="filter">
                <div id="divDirectory" class="blockContent">
                    <span class="headerLabel">Directory Listing:</span><br />
                    <select data-ng-model="view" class="form-control" data-style="btn-primary">
                        <option value="All">All</option>
                        <option value="iPC">Profit Centers</option>
                        <option value="iRM">Region Managers</option>
                        <option value="iRS">Region Support</option>
                        <option value="iSC">Service Centers</option>
                    </select>
                </div>
                <div class="panel-group" id="entryAccordion" role="tablist">
                    <div data-ng-repeat="(firstLetter,PhoneData) in filteredPhoneData" id="alpha{{firstLetter}}">
                        <h4>{{firstLetter}}</h4>
                        <div class="panel panel-default phoneGroup" data-ng-repeat="phoneEntry in PhoneData">
                            <div class="panel-heading phoneHeading" role="tab" id="heading{{$parent.$index}}-{{$index}}">
                                <div class="row" >
                                    <div class="col-xs-7" style="cursor: pointer" data-toggle="collapse" data-parent="#entryAccordion" data-target="#collapse{{$parent.$index}}-{{$index}}">{{phoneEntry.N}} ({{phoneEntry.PC}})</div>
                                    <div class="col-xs-5" style="text-align: center">
                                        <a href="tel:{{phoneEntry.P}}">{{phoneEntry.P}}</a>
                                    </div>
                                </div>
                            </div>
                            <div id="collapse{{$parent.$index}}-{{$index}}" class="panel-collapse collapse" role="tabpanel">
                                <div class="panel-body">
                                    {{phoneEntry.N}}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
