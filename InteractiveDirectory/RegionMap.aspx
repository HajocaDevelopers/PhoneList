<%@ Page Title="Region Map" Language="C#" MasterPageFile="~/RegionMap.Master" AutoEventWireup="true" CodeBehind="RegionMap.aspx.cs" Inherits="InteractiveDirectory.WebForm1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    </asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="regionMap" ng-app="regionMap">
        <div id="mainContainer" ng-controller="regionMapCtrl">
            <div id="pageHeader">
                <div id="divHeaderLeft" class="blockContent">
                    <div id="divHeaderImages" class="blockContent">
                        <img id="ajaxLoader" ng-show="false" src="styles/images/ajax-loader-orange-bar.gif" />
                        <img src="styles/images/HajocaLogo.gif" />
                    </div>
                    <div id="divPageTitle" class="blockContent">
                        Hajoca Region Map
                    </div>
                </div>
            </div>
            <ul class="nav nav-tabs">
                <li class="nav-item">
                    <a class="nav-link" href="default">Directory</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link active">Region Map</a>
                </li>
            </ul>
            <div>
                <div id="accordionMenu" role="tablist" class="col-md-3 col-sm-12 panel-group">
                    <div class="panel panel-default" ng-repeat="(reg, RegionName) in profitcenters | groupBy: 'RegionName'">
                        <div class="panel-heading" role="tab" id="{{reg}}" style="">
                            <div style="height: 3.5em; width: 2em; background-color: {{getColor(reg)}}; float: left; margin: 0 1em 0 0"></div>
                            <h4 class="panel-title">
                                <a class="collapsed" role="button" data-toggle="collapse" data-parent="#{{reg}}" href="#{{filterlabel(reg)}}-content" aria-expanded="false" aria-controls="reg">{{ reg }}</a>

                            </h4>
                        </div>
                        <div id="{{filterlabel(reg)}}-content" class="panel-collapse collapse" role="tabpanel" aria-labelledby="{{reg}}">
                            <div class="panel-body">

                                <button ng-repeat="profitcenter in RegionName" class="panel">
                                    <a id="pc{{profitcenter.PC}}" href="javascript:function(){return false;}">({{profitcenter.PC}}) {{profitcenter.DivsionName}} {{profitcenter.City}}</a>
                                </button>


                            </div>
                        </div>
                    </div>
                </div>
                <div id="map" class="col-sm-12 col-md-9"></div>
            </div>
        </div>

    </div>
</asp:Content>
