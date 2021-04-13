var app = angular.module('interactiveDirectoryApp', ['ui.bootstrap', 'ngCookies', 'ngResource']);
var watchSlowLoad;  // Holds the Angular Promise for our $Timeout to popup the ajaxLoader when things are slow.

app.config(function ($anchorScrollProvider, $tooltipProvider) {
    $anchorScrollProvider.disableAutoScrolling();
    $tooltipProvider.options({ popupDelay: 500 });
  });

app.filter('iif', function () {
    return function (input, trueValue, falseValue) {
        return input ? trueValue : falseValue;
    };
});

app.controller('appCtrl', function ($scope, $timeout, $http, $anchorScroll, $modal) {
    $scope.filter = ""; // Tracks the filter.
    $scope.sort = ""; // Tracks the sort value.
    $scope.view = 0; // Tracsk the current view (Directory Listing).
    $scope.currentPage = 1; // Tracks the current page.
    $scope.totalRecords = 0; // Tracks the total number of records.
    $scope.pageSize = 20; // Tracks the entries shown per page.
    $scope.expandedList = [];  // Tracks the list of expanded records but key.
    $scope.maxSize = 5;
    $scope.filterPrompt = 'Enter Filter Value';
    $scope.doneInitialLoading = false; // Used to hide the ugly empty grid and footer on initial load.
    $scope.processCount = 0;  // Counts the number of outstanding requests stacked on the server.
    $scope.isProcessSlow = false; // We flip this to true if something is going to take a while.  Works with processCount.
    $scope.showMobile = false;

    // ** Filter Changes ** 
    $scope.$watch(
        function () { return $scope.filter; },
        function (newvalue, oldvalue) {
            if ((newvalue.length >= 3) && newvalue != $scope.filterPrompt) UpdateDirecotry($scope, $timeout, $http, true);
            else if ((newvalue.length <= 3) && (oldvalue.length >= 3)) UpdateDirecotry($scope, $timeout, $http);
        }, true);

    // ** Page Size Changes ** 
    $scope.$watch(
        function () { return $scope.pageSize; },
        function (newvalue, oldvalue) {
            if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, true);
        }, true);

    // ** View (Directory Listing) Changes ** 
    $scope.$watch(
        function () { return $scope.view; },
        function (newvalue, oldvalue) {
            if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, true);
        }, true);

    // ** Sort Changes ** 
    $scope.$watch(
    function () { return $scope.sort; },
    function (newvalue, oldvalue) {
        if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, true);
    }, true);

    // ** Page Number Changes ** 
    $scope.$watch(
    function () { return $scope.currentPage; },
    function (newvalue, oldvalue) {
        if (newvalue != oldvalue) UpdateDirecotry($scope, $timeout, $http, false);
    }, true);

    // ** Row View Expanded Toggle Handlers ** 
    $scope.ExpandToggle = function (rowIndex) {
        elementLocation = $scope.expandedList.indexOf(rowIndex)
        if (elementLocation > -1)
            $scope.expandedList.splice(elementLocation, 1);
        else
            $scope.expandedList.push(rowIndex);
    };

    $scope.ExpandAll = function () {
        $scope.expandedList = [];
        for (i = 0; i < $scope.phoneData.length; i++) {
            $scope.expandedList.push(i);
        }
    };

    $scope.CollapseAll = function () {
        $scope.expandedList = [];
    };

    $scope.ShowExpanded = function (rowIndex) {
        return $scope.expandedList.indexOf(rowIndex) > -1
    };
    
    // Tooltips should only be shown on non-portable device since they are the
    // only devices with mice to make it worth the time.  There are also issues
    // with popups being left stranded when a person uses "Back" to come back 
    // to a page that had a popup showing.  The hidden hidHideTooltips variable
    // is populated by the server-side code.
    $scope.ShowTooltip = function (message) {
        if (($("#hidHideTooltips").val())== 'True')
            return ""
        else
            return message
    };

    // Map popup handler
    $scope.OpenMap = function (Address) {
        window.open("http://maps.google.com/?q=" + Address);
    };


    // Excel Export button.
    $scope.exportExcel = function () {
        GetCSV($scope, $http)
    }


    $scope.showTips = function () {
        var modalInstance = $modal.open({
            templateUrl: 'Tips.html'
        });
    };

    // We do not bring data down from the server so we now need to go
    // out and get it!
    UpdateDirecotry($scope, $timeout, $http);
});

app.directive('focusMe', function () {
    return {
        link: function (scope, element, attrs) {
            scope.$watch(attrs.focusMe, function (value) {
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
function UpdateDirecotry($scope, $timeout, $http, resetPage, resetCollapse)
{
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
    if ($scope.processCount == 0) watchSlowLoad = $timeout(function () { $scope.isProcessSlow = true; }, 300);

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
    $http.post('api/directory/GetCurrentDirectory', { filter: localFilter, view: $scope.view, sort: $scope.sort, page: $scope.currentPage, pageSize: $scope.pageSize }).success(function (data, status, headers, config) {
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
        }
    }).
    error(function (data, status, headers, config) {
        // Uh Oh.
        $scope.processCount -= 1;
        if ($scope.processCount == 0) {
            $timeout.cancel(watchSlowLoad);
            $scope.isProcessSlow = false;
            alert("Application could not load data.  Please reload to try again.");
        }
    });
}


function GetCSV($scope, $http) {
    var filter = $scope.filter;
    if (filter.length < 3) filter = "";
    
    window.location.href = "api/directory/GetExcelExport?sort=" + $scope.sort + "&filter=" + filter + "&view=" + $scope.view
}

