function moveToNext(elementId) {
    var nextElement = document.getElementById(elementId);
    if (nextElement) {
        nextElement.focus();
    }
}

function hideElementsAfterDelay(id, delay) {    
    setTimeout(function() {
        var frameErr = document.getElementById(id);
        if (frameErr)
        {
            frameErr.style.display = 'none';
        }
    }, delay);
}

function closeTab() {
    console.log("Da call");
    
    window.close();
}