var app = angular.module('interactiveDirectoryApp', ['ngCookies']);

// The delay below sets the number of milliseconds to wait for Angular to update the listview that
// contains the list of names.  We need to wait for Angular to complete and then run a .Refresh on the
// view to update the header breaks.  Because we're already in a $digest we can't just run .$Apply()
// so instead we just give the browser some time to finish up.
var delay = 100; 

app.filter('iif', function () {
    return function (input, trueValue, falseValue) {
        return input ? trueValue : falseValue;
    };
});


app.controller('appCtrlMobile', function ($scope, $timeout, $http, $cookies) {
    // If the user refreshes on a detail page, we need to reset it becuase we'll have lost
    // the angularJS internal values.
    window.location.hash = "";

    $("#listMobile").hide(); // We hide and show the list so we don't see all the junk from angular and jquery mobile not playing well.
    $scope.listing = "people";         // Tracks the current Directory Listing type.
    $scope.listingDisplay = "";     // What the user sees in the header based on lookup.
    $scope.listingLookup = [        // This is our lookup to show in the heager.  We lobar it FilterDirectory().
          { 'listing': 'people', 'listingDisplay': 'Hajoca Interactive Directory' },
          { 'listing': 'iPC', 'listingDisplay': 'Profit Centers' },
          { 'listing': 'iPM', 'listingDisplay': 'PC Managers' },
          { 'listing': 'iRM', 'listingDisplay': 'Region Managers' },
          { 'listing': 'iRS', 'listingDisplay': 'Region Support' },
          { 'listing': 'iSC', 'listingDisplay': 'Service Center Employees' },
    ];
    $scope.view = "0";

    // Retrieve from cookies:
    if ($cookies['view'] != null) $scope.view = $cookies['view']; // Standard View
    if ($cookies['listing'] != null) $scope.listing = $cookies['listing']; // Standard View

    $(document).ready(function () {
        $(".jump-button").click(function (event) {
            // Avoid looking for something that's not there!
            if ($(".ui-li-divider").length == 0) return;
            
            // Gets the letter of the position we want off the id.  Example: jumpTo_A woudl be "A"
            var letter = event.target.id.split("_").pop();

            // Search the alphabet going forwards and backwards.  If we don't find the letter in the list
            // we go to the next one, if we get all the way to Z then we go backwards until we find one.
            // Example: If there were only names with A and B and we search for N we'd wind up scrolling 
            // to B since C-Z doesn't exist!
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZYXVUTSRQPONMLKJIHGFEDCBA";
            var pos = alphabet.indexOf(letter);
            while ($(".ui-li-divider:contains('" + letter + "')").length == 0) {
                pos = pos + 1
                if (pos >= alphabet.length) return // Shouldn't happen!
                letter = alphabet.substring(pos, pos + 1);
            }

            $('html, body').animate({
                scrollTop: $(".ui-li-divider:contains('" + letter + "')").offset().top
            }, 200);
        });
    });

    // selectedPhone is used by the Details Page so to set all
    // the information on the page we just set it to the phoneEntry
    // selected on the page.  This works for now because we bring all
    // the data down at once.  If we decide to not bring it all down
    // in the future this is where the $http.get would go!
    $scope.setSelectedPhone = function (phoneEntry) {
        $scope.selectedPhone = phoneEntry;
    }
    
    $scope.doneInitialLoading = false; // Used to hide the ugly empty info on initial load.

    // Set the cookie to remember the view.
    $scope.$watch(
       function () { return $scope.view; },
       function (newvalue, oldvalue) {
           $cookies['view'] = newvalue;
       }, true);

    // ** Directory Listing Changes ** 
    $scope.$watch(
        function () { return $scope.listing; },
        function (newvalue, oldvalue) {
            if (newvalue == "undefined")
                return;
            $cookies['listing'] = newvalue;
            $('input[data-type="search"]').val("");
            $('input[data-type="search"]').trigger("keyup");
            FilterDirectory($scope);
        }, true);

    // To dial the phone we should only need to redirect to a page with a tel: prefix and
    // the browser should pick up on it.
    $scope.OpenPhone = function (Phone) {
        window.location.href = "tel:" + Phone;
    };

    // To trigger a new email we only need to add a mailto: to the address.
    $scope.OpenEmail = function (Email) {
        window.location.href = "mailto:" + Email;
    };

    // Not sure how all devices will repspond so we first fire to "maps:" to allow
    // some browsers to pick up on this and fire their maps app.  If that fails to 
    // get recognition the second href will fire to go to google maps.  iPhones will
    // use the first method and never get to the second one--PC browsers like chrome 
    // won't know what to do with the first one and fail through to the second.
    $scope.OpenMap = function (Address) {
        window.location.href = "maps:q=" + Address;
        window.location.href = "http://maps.google.com/?q=" + Address; // if maps: fails to trigger we to http: instead.
    };

    // We do not bring data down from the server so we now need to go out and get it!
    LoadDirecotry($scope, $timeout, $http);
});

// This is the main fetch for data.  It is an asynch process that will call
// the server API for data and handle the return.
function LoadDirecotry($scope, $timeout, $http) {
    // Go get the data in a GET.
    $http.get('api/directory/GetCurrentDirectoryForMobile').success(function (data, status, headers, config) {
        $scope.phoneData = data;
        FilterData($scope)
        setTimeout(function () {
            $("#listMobile").listview("refresh");
            $("#mainContent").show();
            $("#waitContent").hide();
        }, delay);
    }).
    error(function (data, status, headers, config) {
        // Uh Oh.
        alert("Application could not load data.  Please reload to try again.");
    });
}

function FilterData($scope){
    if ($scope.listing == "people") // This is the only hard-coded value since "people" are all entries NOT a Profit Centery.
        $scope.filteredPhoneData = _.filter($scope.phoneData, ['iPC', false]);
    else
        $scope.filteredPhoneData = _.filter($scope.phoneData, $scope.listing);
}

// This uses lobar to filter the Directory Listing type without having to go back to the server.
// We have phoneData to hold everything, but filteredPhoneData holds the objects we show.
// We also use lobar to find the verbose text to put in the title for the filtered directory.
function FilterDirectory($scope) {
    $("#waitContent").show();
    $("#listMobile").hide();

    // Filter or restore the data.
    FilterData($scope)

    // Set the header display.
    $scope.listingDisplay = _.find($scope.listingLookup, ['listing', $scope.listing]).listingDisplay;
    setTimeout(function () {
        $("#listMobile").listview("refresh");
        $("#listMobile").show();
        $("#waitContent").hide();
    }, delay);
}

