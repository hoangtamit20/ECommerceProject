function moveToNext(elementId) {
    var nextElement = document.getElementById(elementId);
    if (nextElement) {
        nextElement.focus();
    }
}

function hideElementsAfterDelay(id, delay) {
    setTimeout(function () {
        var frameErr = document.getElementById(id);
        if (frameErr) {
            frameErr.style.display = 'none';
        }
    }, delay);
}

function getCurrentUrl() {
    return window.location.href;
}

function closeTab() {
    window.close();
}


window.initializeSidebar = () => {
    const hamBurger = document.querySelector(".toggle-btn");

    if (hamBurger) {
        hamBurger.addEventListener("click", function () {
            document.querySelector("#sidebar").classList.toggle("expand");
        });
    }
};
