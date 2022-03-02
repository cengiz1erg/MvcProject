// Initialize and add the map
function initMap2() {
    // The location of Uluru
    const uluru = { lat: myJsVariableX, lng: myJsVariableY };
    // The map, centered at Uluru
    const map2 = new google.maps.Map(document.getElementById("map2"), {
        zoom: 10,
        center: uluru,
    });
    // The marker, positioned at Uluru
    const marker = new google.maps.Marker({
        position: uluru,
        map: map2,
    });
}
