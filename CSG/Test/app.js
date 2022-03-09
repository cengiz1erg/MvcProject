
        var Iyzipay = require('iyzipay');

        var iyzipay = new Iyzipay({
            apiKey: "sandbox-zCEECkYFMbaWcVq3d6O9bNfL4o4YJLxA",
            secretKey: "sandbox-MNmiw8ZolOIILD2VBluaIluW5QI6UUWe",
            uri: 'https://sandbox-api.iyzipay.com'
        });

        iyzipay.installmentInfo.retrieve({
            locale: Iyzipay.LOCALE.TR,
            conversationId: '123456789',
            binNumber: '554960',
            price: '100'
        }, function (err, result) {
            console.log(result);
            console.log(result.installmentDetails[0].installmentPrices)
        });
