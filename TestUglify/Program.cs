using System;
using NUglify;

// Reproduce the exact async runFadeTransition block
var js = @"
async function runFadeTransition() {
    try {
        const fadeOut = textElem.animate(
            [{opacity: 1}, {opacity: 0}],
            {duration: FADE_DURATION_MS, easing: 'ease', fill: 'forwards'}
        );
        await fadeOut.finished;
        textElem.textContent = getDisplayString();
        updateTextSize();
        await new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)));
        const fadeIn = textElem.animate(
            [{opacity: 0}, {opacity: 1}],
            {duration: FADE_DURATION_MS, easing: 'ease', fill: 'forwards'}
        );
        await fadeIn.finished;
        fadeOut.cancel();
        fadeIn.cancel();
    } finally {
        transitioning = false;
    }
}
";
var result = Uglify.Js(js);
Console.WriteLine("ERRORS: " + string.Join(", ", result.Errors));
Console.WriteLine("CODE: " + result.Code);
