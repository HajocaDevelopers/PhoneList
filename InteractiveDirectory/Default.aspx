<%@ Page Title="Hajoca Interactive Directory" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="InteractiveDirectory.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <input type="hidden" id="hidHideTooltips" value="<%=Request.Browser.IsMobileDevice%>">
    <div id="mainContainer">
        <div id="pageHeader">
            <div id="divHeaderLeft" class="blockContent">
                <div id="divHeaderImages" class="blockContent">
                    <img id="ajaxLoader" ng-show="(processCount>0) && (isProcessSlow)" src="styles/images/ajax-loader-orange-bar.gif" />
                    <img src="styles/images/HajocaLogo.gif" />
                </div>
                <div id="divPageTitle" class="blockContent">
                    Hajoca Interactive Directory
                </div>
            </div>
            <div id="divHeaderRight" class="blockContent">
                <div id="divDirectory" class="blockContent">
                    <span class="headerLabel">Directory Listing:</span><br />
                    <select ng-model="view" class="form-control" data-style="btn-primary">
                        <option value="0">All</option>
                        <option value="1">Profit Centers</option>
                        <option value="2">Region Managers</option>
                        <option value="3">Region Support</option>
                        <option value="4">National Support Center</option>
                    </select>
                </div>
                <div id="divFilter" class="blockContent">
                    <span class="headerLabel">Filter:</span><br />
                    <input type="text" focus-me="true" id="filter" placeholder="{{filterPrompt}}" ng-model="$parent.filter" tooltip="{{ShowTooltip('Type at least 3 characters to filter the results.')}}" tooltip-placement="bottom" tooltip-popup-delay="0" />
                </div>
                <div id="divPerPage" class="blockContent">
                    <span class="headerLabel">Entries Per Page:</span><br />
                    <select ng-model="pageSize" class="form-control" data-style="btn-primary">
                        <option value="10">10</option>
                        <option value="20">20</option>
                        <option value="100">100</option>
                        <option value="0">All</option>
                    </select>
                </div>
                <div id="divTips" class="blockContent">
                    <img width="35" height="35" src="styles/images/tips.gif" ng-click="showTips()" tooltip="{{ShowTooltip('Click to view some quick tips.')}}" tooltip-placement="left" />
                </div>
                <div id="divExcelDownload" class="blockContent">
                    <img width="33" height="35" src="styles/images/ExcelDownloadIcon.png" ng-click="exportExcel()" tooltip="{{ShowTooltip('Click to create a CSV of the current data to open in Excel.')}}" tooltip-placement="left" />
                </div>
            </div>
        </div>
        <div id="searchResults" ng-show="doneInitialLoading">
            <div id="divTableContainer" class="table-responsive">
                <table id="mainTable" class="table table-hover table-condensed">
                    <thead>
                        <tr class="phoneDataHeader">
                            <th class="buttonColumn">
                                <a href="#" ng-click="CollapseAll()" tooltip="Collapse All" tooltip-placement="right"><span class="glyphicon glyphicon glyphicon-collapse-up"></span></a>
                                <a href="#" ng-click="ExpandAll()" tooltip="Expand All" tooltip-placement="right"><span class="glyphicon glyphicon glyphicon-collapse-down"></span></a>
                            </th>
                            <th class="columnHeader smallColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by Region Number')}}" tooltip-placement="right" ng-click="$parent.sort='RegionNumberSort,PCSort,Name'">Reg</a></th>
                            <th class="columnHeader smallColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by PC')}}" ng-click="$parent.sort='PCSort,Name'">PC</a></th>
                            <th class="columnHeader mediumColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by Name')}}" ng-click="$parent.sort='Name'">Name</a></th>
                            <th class="columnHeader mediumColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by Phone')}}" ng-click="$parent.sort='Phone'">Phone #</a></th>
                            <th class="columnHeader mediumColumnHeader{{showMobile | iif : 'Mobile' : ''}}" ng-show="showMobile"><a href="#" tooltip="{{ShowTooltip('Sort by Mobile Phone')}}" ng-click="$parent.sort='Mobile'">Mobile #</a></th>
                            <th class="columnHeader largeColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by PC Manager / Department')}}" ng-click="$parent.sort='ManagerDepartment,PCSort,Name'">PC Manager / Department</a></th>
                            <th class="columnHeader largeColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by Email Address')}}" ng-click="$parent.sort='EmailAddress'">Email Address</a></th>
                            <th class="columnHeader largeColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by Address')}}" ng-click="$parent.sort='Street'">Address</a></th>
                            <th class="columnHeader largeColumnHeader{{showMobile | iif : 'Mobile' : ''}}"><a href="#" tooltip="{{ShowTooltip('Sort by City/State/Zip')}}" ng-click="$parent.sort='CSZ'">City/State/Zip</a></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr ng-repeat-start="phoneEntry in phoneData" ng-class-odd="'phoneDataOdd'" ng-class-even="'phoneDataEven'">
                            <td class="buttonColumn"><a href="#" ng-click="ExpandToggle($index)" tooltip="{{ShowTooltip(ShowExpanded($index) && 'Collapse' || 'Expand')}}" tooltip-placement="right"><span class="glyphicon glyphicon-collapse-down" ng-show="!ShowExpanded($index)"></span><span class="glyphicon glyphicon glyphicon-collapse-up" ng-show="ShowExpanded($index)"></span></a></td>
                            <td><span class="tipSpan" tooltip="{{ShowTooltip(phoneEntry.RegionName)}}" tooltip-placement="right" tooltip-popup-delay="0">{{phoneEntry.RegionNumber}}</span></td>
                            <td><span class="tipSpan" tooltip="{{ShowTooltip(phoneEntry.DivsionName)}}" tooltip-placement="right" tooltip-popup-delay="0">{{phoneEntry.PC}}</span></td>
                            <td>{{phoneEntry.Name}}</td>
                            <td><a href="tel:{{phoneEntry.Phone}}" tooltip="{{ShowTooltip('Dial ' + phoneEntry.Phone)}}">{{phoneEntry.Phone}}</a></td>
                            <td ng-show="showMobile"><a href="tel:{{phoneEntry.Mobile}}"  tooltip="{{ShowTooltip('Dial ' + phoneEntry.Mobile)}}">{{phoneEntry.Mobile}}</a></td>
                            <td>{{phoneEntry.ManagerDepartment}}</td>
                            <td><a href="mailto:{{phoneEntry.EmailAddress}}" tooltip="{{ShowTooltip('Email ' + phoneEntry.EmailAddress)}}">{{phoneEntry.EmailAddress}}</a></td>
                            <td><a href="#" tooltip="{{ShowTooltip('Map to ' + phoneEntry.Street)}}" ng-click="OpenMap(phoneEntry.Street + ',' + phoneEntry.CSZ)">{{phoneEntry.Street}}</a></td>
                            <td><a href="#" tooltip="{{ShowTooltip('Map to ' + phoneEntry.CSZ)}}" ng-click="OpenMap(phoneEntry.CSZ)">{{phoneEntry.CSZ}}</a></td>
                        </tr>
                        <tr ng-repeat-end="" ng-show="ShowExpanded($index)">
                            <td colspan="{{showMobile | iif : '10' : '9'}}" class="detailTableHolderCell">
                                <div class="detailLinkIcon"><img src="styles/images/left-arrow-icon.gif" /></div>
                                <table id="detailTable" class="table table-bordered table-hover table-condensed">
                                    <tr class="detailRow">
                                        <td class="detailLabel" rowspan="2">PO Box:</td>
                                        <td class="detailValueLarge">{{phoneEntry.POBox}}</td>
                                        <td class="detailLabel">Eclipse Number:</td>
                                        <td class="detailValueSmall">{{phoneEntry.EclipseNumber}}</td>
                                        <td class="detailLabel">Eclipse Box:</td>
                                        <td class="detailValueSmall">{{phoneEntry.EclipseBox}}</td>
                                        <td class="detailLabel">Credit Manager:</td>
                                        <td class="detailValueLarge" colspan="3">{{phoneEntry.CreditManager}}</td>
                                    </tr>
                                    <tr class="detailRow">
                                        <td class="detailValueLarge">{{phoneEntry.POCSZ}}</td>
                                        <td class="detailLabel">On-Site Contact:</td>
                                        <td class="detailValueLarge" colspan="3">{{phoneEntry.OnSiteContact}}</td>
                                        <td class="detailLabel">Credit Mgr Phone:</td>
                                        <td class="detailValueSmall"><a href="tel:{{phoneEntry.CreditMgrPhone}}" tooltip="{{ShowTooltip('Dial ' + phoneEntry.CreditMgrPhone)}}">{{phoneEntry.CreditMgrPhone}}</a></td>
                                        <td class="detailLabel">Credit Mgr Fax:</td>
                                        <td class="detailValueSmall"><a href="tel:{{phoneEntry.CreditMgrFax}}" tooltip="{{ShowTooltip('Dial ' + phoneEntry.CreditMgrFax)}}">{{phoneEntry.CreditMgrFax}}</a></td>
                                    </tr>
                                    <tr class="detailRow">
                                        <td class="detailLabel">Fax Number:</td>
                                        <td class="detailValueLarge"><a href="tel:{{phoneEntry.Fax}}" tooltip="{{ShowTooltip('Dial ' + phoneEntry.Fax)}}">{{phoneEntry.Fax}}</a></td>
                                        <td class="detailLabel">On-Site Contact Email:</td>
                                        <td class="detailValueLarge" colspan="3"><a href="mailto:{{phoneEntry.OnSiteContactEmail}}" tooltip="{{ShowTooltip('Email ' + phoneEntry.OnSiteContactEmail)}}">{{phoneEntry.OnSiteContactEmail}}</a></td>
                                        <td class="detailLabel">Credit Mgr Email:</td>
                                        <td class="detailValueSmall" colspan="3"><a href="mailto:{{phoneEntry.CreditMgrEmail}}" tooltip="{{ShowTooltip('Email ' + phoneEntry.CreditMgrEmail)}}">{{phoneEntry.CreditMgrEmail}}</a></td>
                                    </tr>
                                    <tr class="detailRow">
                                        <td class="detailLabel">Division:</td>
                                        <td class="detailValueLarge">{{phoneEntry.DivsionName}}</td>
                                        <td class="detailLabel">Region:</td>
                                        <td class="detailValueLarge" colspan="3">{{phoneEntry.RegionNumber}} <span ng-if="phoneEntry.RegionName.length>0" >-</span> {{phoneEntry.RegionName}}</td>
                                        <td class="detailLabel">Keywords:</td>
                                        <td class="detailValueSmall" colspan="3">{{phoneEntry.Keywords}}</td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                    <tfoot ng-show="doneInitialLoading && totalRecords==0">
                        <td colspan="10">Your Search Did Not Return Any Results</td>
                    </tfoot>
                </table>
            </div>
        </div>
        <div id="pageFooter" ng-show="doneInitialLoading && pageSize!=0" style="height:30px;">
            <div id="divPageCount" class="blockContent">
                <pre>Page: {{currentPage}} / {{totalPages}}</pre>
            </div>
            <div id="divPagination" class="blockContent">
                <pagination total-items="totalRecords" ng-model="currentPage" max-size="maxSize" items-per-page="pageSize" class="pagination-sm bottomPagination" boundary-links="true" num-pages="totalPages"></pagination>
            </div>
        </div>
    </div>
</asp:Content>