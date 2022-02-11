var map = null;
var marker = null;

function initMap() {
    console.log("Harita Y�klendi.");
    const myLatlng = { lat: 41.0441, lng: 29.0067 };
    map = new google.maps.Map(document.getElementById("map"), {
        zoom: 15,
        center: myLatlng
    });
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const pos = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude,
                };
                map.setCenter(pos);
            },
            () => {
                handleLocationError(true, infoWindow, map.getCenter());
            }
        );
    } else {
        // Browser doesn't support Geolocation
        handleLocationError(false, infoWindow, map.getCenter());
    }

    map.addListener("click", (mapsMouseEvent) => {
        console.log(mapsMouseEvent);
        if (marker == null) {
            marker = new google.maps.Marker({
                position: mapsMouseEvent.latLng,
                map: map
            });
        }
        else {
            marker.setMap(null);
            marker = new google.maps.Marker({
                position: mapsMouseEvent.latLng,
                map: map
            });
        }
        $("#loc-x").attr("value", String(mapsMouseEvent.latLng.lat()));
        $("#loc-y").attr("value", String(mapsMouseEvent.latLng.lng()));
        console.log("Enlem: " + mapsMouseEvent.latLng.lat());
        console.log("Boylam: " + mapsMouseEvent.latLng.lng()); 

    });
}