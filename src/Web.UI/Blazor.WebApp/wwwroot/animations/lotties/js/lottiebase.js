// lottiebase.js
window.loadLottieAnimation = function (path) {
    var animation = bodymovin.loadAnimation({
        container: document.getElementById('lottie-animation'),
        path: `animations/lotties/jsons/${path}`,
        renderer: 'svg',
        loop: true,
        autoplay: true,
        name: "Lottie Animation",
    });
};
