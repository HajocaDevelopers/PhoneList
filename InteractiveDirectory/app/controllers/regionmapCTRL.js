var regionMap = angular.module('regionMap', ['ui.bootstrap', 'angular.filter'])
    .config(['$locationProvider', function ($locationProvider) {
        $locationProvider.html5Mode({
            enabled : true,
            requireBase: false});
    }]);

regionMap.controller('regionMapCtrl', function ($scope, $window, $http, $location) {
    $scope.result = [];
    $scope.profitcenters = [];
    $scope.itemsPerPage = 15;
    $scope.currentPage = 0;
	
    //var searchObject = $location.search();
    var searchObject = {};
    if (searchObject.address == null) {
        $scope.searchquery = "Ex: Philadelphia, PA or 19131";
    } else {
        $scope.searchquery = searchObject.address;
    }
    var term = $scope.searchquery;
    var geocoder, map, marker;
    function initialize(lat, lng, zoomin) {
        geocoder = new google.maps.Geocoder();
        var latlng = new google.maps.LatLng(lat, lng);
        var mapOptions = {
            zoom: zoomin,
            center: latlng
        };
        map = new google.maps.Map(document.getElementById("map"), mapOptions);
        for (i = 0; i < $scope.profitcenters.length; i++) {
            marker = new google.maps.Marker({
                position: new google.maps.LatLng($scope.profitcenters[i].latitude, $scope.profitcenters[i].longitude),
                //title: $scope.profitcenters.Division,
                map: map
            });
        }
    }

    function getResults() {
        $http.post('http://localhost/interactivedirectory/api/directory/getProfitCenterDirectory/?view=0').then(function (response) {
            $scope.result = response;
            $scope.profitcenters = response.data;
            initialize(39.3895416000, -101.0364161000, 5);
        });
    }
    getResults();

    $scope.getSearch = function getSearch(term) {
        geocoder.geocode({ 'address': term }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                $http.get('https://hajoca.com/api/profitcenter/zip?lat=' + results[0].geometry.location.lat() + "&lng=" + results[0].geometry.location.lng()).then(function (data) {
                    $scope.profitcenters = data.data;
                    if ($scope.profitcenters.length == 0) {
                        initialize(39.3895416000, -101.0364161000, 4);
                    } else {
                        initialize($scope.profitcenters[0].Latitude, $scope.profitcenters[0].Longitude, 10);
                    }
                });
            } else {
                // alert($scope.profitcenters.length);
            }
        });
    };
	$scope.clearSearch = function clearSearch(){
		$scope.searchquery = "";
	};
    
    
});
regionMap.filter('tel', function () {
    return function (tel) {
        if (!tel) { return ''; }

        var value = tel.toString().trim().replace(/^\+/, '');

        if (value.match(/[^0-9]/)) {
            return tel;
        }

        var country, city, number;

        switch (value.length) {
            case 10: // +1PPP####### -> C (PPP) ###-####
                country = 1;
                city = value.slice(0, 3);
                number = value.slice(3);
                break;

            case 11: // +CPPP####### -> CCC (PP) ###-####
                country = value[0];
                city = value.slice(1, 4);
                number = value.slice(4);
                break;

            case 12: // +CCCPP####### -> CCC (PP) ###-####
                country = value.slice(0, 3);
                city = value.slice(3, 5);
                number = value.slice(5);
                break;

            default:
                return tel;
        }

        if (country == 1) {
            country = "";
        }

        number = number.slice(0, 3) + '-' + number.slice(3);

        return (country + " (" + city + ") " + number).trim();
    };
});
