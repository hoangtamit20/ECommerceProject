var animation = bodymovin.loadAnimation({
    container: document.getElementById('lottie-animation'), // container để chứa animation
    path: 'animations/lotties/jsons/success.json', // đường dẫn đến file JSON
    renderer: 'svg', // định dạng renderer (svg, canvas, html)
    loop: true, // có lặp lại animation hay không
    autoplay: true, // có tự động chạy animation hay không
    name: "Lottie Animation", // tên của animation
});
