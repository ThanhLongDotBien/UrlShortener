// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const copyButtons = document.querySelectorAll(".copy-btn");

    copyButtons.forEach((button) => {
        button.addEventListener("click", async function () {
            const text = button.getAttribute("data-copy");
            if (!text) return;

            try {
                await navigator.clipboard.writeText(text);
                const originalText = button.textContent;
                button.textContent = "Copied";
                setTimeout(() => {
                    button.textContent = originalText;
                }, 1500);
            } catch {
                alert("Copy failed. Please copy the link manually.");
            }
        });
    });
});
