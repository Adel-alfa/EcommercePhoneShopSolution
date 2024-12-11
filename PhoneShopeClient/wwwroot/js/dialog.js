window.ShowDialog = function () {
    document.getElementById('my-dialog').showModal()
}

/* for IPV4*/
/*and for the IPV6 you can use fetch('https://jsonip.com/') */
 async function getClientIp() {
            const response = await fetch('https://api.ipify.org?format=json');
                                const data = await response.json();
                                return data.ip;
        }
             