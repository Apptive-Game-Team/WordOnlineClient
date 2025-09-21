function resizeCanvas() {
    var canvas = document.querySelector("#unity-canvas");
    var warning = document.querySelector("#rotate-warning");

    var windowAspect = window.innerWidth / window.innerHeight;
    var targetAspect = 16 / 9; // 원하는 비율

    // 모바일 세로모드 감지
    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent) && window.innerHeight > window.innerWidth) {
        warning.style.display = "block";
        canvas.style.display = "none";
        return;
    } else {
        warning.style.display = "none";
        canvas.style.display = "block";
    }

    // Canvas 비율 유지
    if (windowAspect > targetAspect) {
        canvas.style.height = window.innerHeight + "px";
        canvas.style.width = (window.innerHeight * targetAspect) + "px";
    } else {
        canvas.style.width = window.innerWidth + "px";
        canvas.style.height = (window.innerWidth / targetAspect) + "px";
    }
}

window.addEventListener("resize", resizeCanvas);
window.addEventListener("load", resizeCanvas);
