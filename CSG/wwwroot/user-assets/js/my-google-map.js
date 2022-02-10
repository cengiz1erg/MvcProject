function konumBul() {
    if (navigator.geolocation) {
        console.log("Geolocation destekliyor");
        navigator.geolocation.getCurrentPosition(
            function (data) {
                console.log(data);
                var directionsService = new google.maps.DirectionsService();
                var directionsRenderer = new google.maps.DirectionsRenderer();
                const evKonum = {
                    lat: 40.9583073,
                    lng: 29.102783,
                };
                const konum = {
                    lat: data.coords.latitude,
                    lng: data.coords.longitude,
                };
                const haritaDiv = document.getElementById("map");
                const googleMap = new google.maps.Map(haritaDiv, {
                    center: konum,
                    zoom: 15,
                    //mapTypeId: "satellite",
                });
                // const marker = new google.maps.Marker({
                //     position: konum,
                //     map: googleMap,
                //     title:'Þu an buradasýnýz'
                // });

                directionsRenderer.setMap(googleMap);

                directionsService
                    .route({
                        origin: konum,
                        destination: evKonum,
                        travelMode: google.maps.TravelMode.DRIVING,
                    })
                    .then((response) => {
                        directionsRenderer.setDirections(response);
                    })
                    .catch((e) =>
                        window.alert("Directions request failed due to " + status)
                    );
            },
            function (error) {
                alert(error.message);
            }
        );
    } else {
        alert("Geolocation desteklemiyor");
    }
}

konumBul();