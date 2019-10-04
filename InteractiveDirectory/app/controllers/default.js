﻿var app = angular.module('interactiveDirectoryApp', ['ui.bootstrap', 'ngCookies', 'ngResource', 'angular.filter']);
var watchSlowLoad; // Holds the Angular Promise for our $Timeout to popup the ajaxLoader when things are slow.

app.config(function($anchorScrollProvider, $tooltipProvider) {
    $anchorScrollProvider.disableAutoScrolling();
    $tooltipProvider.options({ popupDelay: 500 });
});

app.filter('iif', function() {
    return function(input, trueValue, falseValue) {
        return input ? trueValue : falseValue;
    };
});

app.controller('appCtrl', function($scope, $timeout, $http, $anchorScroll, $modal) {
    $scope.filter = ""; // Tracks the filter.
    $scope.sort = ""; // Tracks the sort value.
    $scope.view = 0; // Tracsk the current view (Directory Listing).
    $scope.currentPage = 1; // Tracks the current page.
    $scope.totalRecords = 0; // Tracks the total number of records.
    $scope.pageSize = 20; // Tracks the entries shown per page.
    $scope.expandedList = []; // Tracks the list of expanded records but key.
    $scope.maxSize = 5;
    $scope.filterPrompt = 'Enter Filter Value';
    $scope.doneInitialLoading = false; // Used to hide the ugly empty grid and footer on initial load.
    $scope.processCount = 0; // Counts the number of outstanding requests stacked on the server.
    $scope.isProcessSlow = false; // We flip this to true if something is going to take a while.  Works with processCount.
    $scope.showMobile = false;

    // ** Filter Changes ** 
    $scope.$watch(
        function() { return $scope.filter; },
        function(newvalue, oldvalue) {
            if ((newvalue.length >= 3) && newvalue != $scope.filterPrompt) UpdateDirecotry($scope, $timeout, $http, true);
            else if ((newvalue.length <= 3) && (oldvalue.length >= 3)) UpdateDirecotry($scope, $timeout, $http);
        }, true);

    // ** Page Size Changes ** 
    $scope.$watch(
        function() { return $scope.pageSize; },
        function(newvalue, oldvalue) {
            if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, true);
        }, true);

    // ** View (Directory Listing) Changes ** 
    $scope.$watch(
        function() { return $scope.view; },
        function(newvalue, oldvalue) {
            if (newvalue != oldvalue) {
                UpdateDirecotry($scope, $timeout, $http, true);
                getResults();
            }
        }, true);

    // ** Sort Changes ** 
    $scope.$watch(
        function() { return $scope.sort; },
        function(newvalue, oldvalue) {
            if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, true);
        }, true);

    // ** Page Number Changes ** 
    $scope.$watch(
        function() { return $scope.currentPage; },
        function(newvalue, oldvalue) {
            if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, false);
        }, true);

    // ** Row View Expanded Toggle Handlers ** 
    $scope.ExpandToggle = function(rowIndex) {
        elementLocation = $scope.expandedList.indexOf(rowIndex)
        if (elementLocation > -1)
            $scope.expandedList.splice(elementLocation, 1);
        else
            $scope.expandedList.push(rowIndex);
    };

    $scope.ExpandAll = function() {
        $scope.expandedList = [];
        for (i = 0; i < $scope.phoneData.length; i++) {
            $scope.expandedList.push(i);
        }
    };

    $scope.CollapseAll = function() {
        $scope.expandedList = [];
    };

    $scope.ShowExpanded = function(rowIndex) {
        return $scope.expandedList.indexOf(rowIndex) > -1
    };

    // Tooltips should only be shown on non-portable device since they are the
    // only devices with mice to make it worth the time.  There are also issues
    // with popups being left stranded when a person uses "Back" to come back 
    // to a page that had a popup showing.  The hidden hidHideTooltips variable
    // is populated by the server-side code.
    $scope.ShowTooltip = function(message) {
        if (($("#hidHideTooltips").val()) == 'True')
            return ""
        else
            return message
    };

    // Map popup handler
    $scope.OpenMap = function(Address) {
        window.open("http://maps.google.com/?q=" + Address);
    };


    // Excel Export button.
    $scope.exportExcel = function() {
        GetCSV($scope, $http)
    }


    $scope.showTips = function() {
        var modalInstance = $modal.open({
            templateUrl: 'Tips.html'
        });
    };

    // We do not bring data down from the server so we now need to go
    // out and get it!
    UpdateDirecotry($scope, $timeout, $http);


    //setup Region Map section
    $scope.profitcenters = [];
    $scope.selectedprofitcenters = [];

    var searchObject = {};
    if (searchObject.address == null) {
        $scope.searchquery = "Ex: Philadelphia, PA or 19131";
    } else {
        $scope.searchquery = searchObject.address;
    }
    var term = $scope.searchquery;
    var geocoder, map, marker;
    //functions for Map info. Gets all ProfitCenters for appropriate view and creates a new map instance
    function getResults() {
        $http.post('api/directory/getProfitCenterDirectory/?view=' + $scope.view).then(function(response) {
            $scope.result = response;
            $scope.profitcenters = response.data;
            initialize(39.3895416000, -101.0364161000, 4);
        });
        //console.log('view: ' + $scope.view);

    }
    $scope.colors = {
        1: '#FF6600',
        2: '#99FF00',
        3: '#006600',
        4: '#009900',
        6: '#66CCFF',
        7: '#0099FF',
        8: '#003366',
        11: '#99CCFF',
        12: '#FF0000',
        13: '#CC0000',
        140: '#660000',
        200: '#FFFFCC',
        300: '#FFFF00',
        301: '#999933',
        305: '#FF99FF',
        306: '#FF00FF',
        308: '#330099',
        310: '#9933FF',
        304: '#663300',
        450: '#99FF99',
        470: '#993333',
        'NORTHEAST REGION': '#FF6600',
        'MOUNTAIN REGION': '#99FF00',
        'EAST CENTRAL REGION': '#006600',
        'MIDWEST REGION': '#009900',
        'SOUTH CENTRAL REGION': '#66CCFF',
        'NORTH CENTRAL REGION': '#0099FF',
        'ARIZONA REGION': '#003366',
        'NORTHWEST REGION': '#99CCFF',
        'SOUTH FLORIDA REGION': '#FF0000',
        'CHESAPEAKE REGION': '#CC0000',
        'NEW VENTURES REGION': '#660000',
        'NORTH FLORIDA REGION': '#FFFFCC',
        'NORTH TEXAS REGION': '#FFFF00',
        'SOUTH TEXAS REGION': '#999933',
        'CAROLINAS REGION': '#FF99FF',
        'SOUTHEAST REGION': '#FF00FF',
        'SOUTHERN CALIF REG': '#330099',
        'SANDALE': '#9933FF',
        'ALL-TEX': '#663300',
        'NEW ENGLAND REGION': '#99FF99',
        'EPSCO': '#993333'
    };

    function initialize(lat, lng, zoomin) {
        geocoder = new google.maps.Geocoder();
        var latlng = new google.maps.LatLng(lat, lng);
        var mapOptions = {
            zoom: zoomin,
            center: latlng
        };
        map = new google.maps.Map(document.getElementById("map"), mapOptions);
        for (i = 0; i < $scope.profitcenters.length; i++) {
            var marker = new google.maps.Marker({
                position: new google.maps.LatLng($scope.profitcenters[i].latitude, $scope.profitcenters[i].longitude),
                title: "PC " + $scope.profitcenters[i].PC,
                map: map,
                icon: {
                    path: google.maps.SymbolPath.BACKWARD_CLOSED_ARROW,
                    scale: 4,
                    fillColor: $scope.colors[$scope.profitcenters[i].RegionName],
                    fillOpacity: 1,
                    strokeColor: '#666',
                    strokeWeight: 1,                  
                },
                //label: $scope.profitcenters[i].PC
                //icon: 'styles/images/green-dot.png',
                //icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
            });
            attachMessage(marker, "<span class='title'>" +
                $scope.profitcenters[i].DivsionName + " " + $scope.profitcenters[i].City + " - (" + $scope.profitcenters[i].PC + ")</span><br/>" + $scope.profitcenters[i].Street + "<br/> " +
                $scope.profitcenters[i].City + ", " + $scope.profitcenters[i].State + "<br/>" +
                "PCM: " + $scope.profitcenters[i].ManagerDepartment + "<br/>" +
                '<a href="http://maps.google.com/?q=' + $scope.profitcenters[i].Street + ',' + $scope.profitcenters[i].CSZ + '" target="_blank"> View in Google Maps</a>');
        }
    }

    function attachMessage(marker, message) {
        var infowindow = new google.maps.InfoWindow({
            content: message
        });

        marker.addListener('click', function() {
            infowindow.open(marker.get('map'), marker);
        });
    }
    getResults();
    $('a[data-toggle="tab"]').on('shown.bs.tab', function(e) {
        e.target // newly activated tab
        getResults();
    });

    $scope.filterlabel = function(label) {
        return label.replace(/\s+/g, '');
    };
    $scope.getColor = function(region) {
        return $scope.colors[region];
    };
    $scope.countpc = function(region) {
        console.log(region);
        return '10';
    };
    $scope.testcount = 10;

});

app.directive('focusMe', function() {
    return {
        link: function(scope, element, attrs) {
            scope.$watch(attrs.focusMe, function(value) {
                if (value === true) {
                    element[0].focus();
                    element[0].select();
                }
            });
        }
    };
});

// This is the main fetch for data.  It is an asynch process that will call
// the server API for data and handle the return.
function UpdateDirecotry($scope, $timeout, $http, resetPage, resetCollapse) {
    // Resetting the currentPage will automatically re-trigger the 
    // UpdateDirecotry so if a reset is request we "return" right
    // away knowing full well it will come right back from the 
    // currentPage trigger.
    if (resetPage & ($scope.currentPage != 1)) {
        $scope.currentPage = 1;
        return;
    }

    // If this is the first call to UpdateDirecotry in the queue we're going to set a timeout
    // to fire if it takes too long for the server to respond.  We store the promise in 
    // watchSlowLoad so we can cancel it later if the data comes back before the timeout fires.
    if ($scope.processCount == 0) watchSlowLoad = $timeout(function() { $scope.isProcessSlow = true; }, 300);

    // If the page size is big we know that the screen build AFTER the data comes back is still 
    // going to  be slow but by then it's too late to turn the ajaxLoader image on so we'll just 
    // turn it on right away.
    if (($scope.pageSize == 0) || ($scope.pageSize > 20)) $scope.isProcessSlow = true;

    // We keep track of how many times the user has requested something from the server in case they
    // put out a second request before the first one comes back since we are running asynch.
    $scope.processCount += 1;

    // The filter has to be 3 in length.  If it's less the 3 we let it alone on the screen
    // but pass an empty string to the server.
    var localFilter = $scope.filter;
    if (localFilter.length < 3) localFilter = "";

    // Go get the data in a POST.
    $http.post('api/directory/GetCurrentDirectory', { filter: localFilter, view: $scope.view, sort: $scope.sort, page: $scope.currentPage, pageSize: $scope.pageSize }).success(function(data, status, headers, config) {
        $scope.processCount -= 1; // Reduce the count for asynch count.
        if ($scope.processCount == 0) // We only update the screen if we don't have any other ascyn requests out there.
        {
            $scope.totalRecords = data.totalRecords;
            $scope.phoneData = data.DirectoryItems;
            $scope.showMobile = data.showMobile;
            $scope.expandedList = []; // We need to reset this so nothing is expanded anymore.
            $scope.doneInitialLoading = true; // This is to show the footer info since we hide it until we've gotten atleast one set of data back.
            $timeout.cancel(watchSlowLoad); // Kill the pop-up if it's not up yet.  Note: It will still remain shown until the screen is done updating.
            $scope.isProcessSlow = false; // We set this back for now.
            //console.log(data);
        }
    }).
    error(function(data, status, headers, config) {
        // Uh Oh.
        $scope.processCount -= 1;
        if ($scope.processCount == 0) {
            $timeout.cancel(watchSlowLoad);
            $scope.isProcessSlow = false;
            alert("Application could not load data.  Please reload to try again.");
        }
    });

    //console.log('view: ' + $scope.view);
    $http.post('api/directory/GetProfitCenterDirectory/?view=' + $scope.view).then(function successCallback(data) {
            $scope.pcinfo = data;
            //console.log($scope.pcinfo.data);
        },
        function errorCallback(data) {
            console.log('Error: ' + data.message);
        });
}


function GetCSV($scope, $http) {
    var filter = $scope.filter;
    if (filter.length < 3) filter = "";

    window.location.href = "api/directory/GetExcelExport?sort=" + $scope.sort + "&filter=" + filter + "&view=" + $scope.view
}

//ToDoList
/* Color markers associated with Division
Rig click events for PC list to filter marker search
Add marker click event to display PC info
*/