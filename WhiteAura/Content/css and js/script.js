function expandSearch() {
    const searchContainer = document.querySelector('.search-container');
    searchContainer.classList.toggle('active');
    if (searchContainer.classList.contains('active')) {
        searchContainer.querySelector('.search-bar').focus();
    }
}

window.addEventListener('click', function (e) {
    const searchContainer = document.querySelector('.search-container');
    if (!searchContainer.contains(e.target)) {
        searchContainer.classList.remove('active');
    }
});

function toggleNav() {
    const sideNav = document.getElementById("side-nav");
    sideNav.classList.toggle("active");
}

window.onscroll = function () {
    const navbar = document.getElementById("navbar");
    if (window.scrollY > 100) { // Adjust the value as needed
        navbar.classList.add("sticky");
    } else {
        navbar.classList.remove("sticky");
    }
};

document.addEventListener('DOMContentLoaded', () => {
    const counters = document.querySelectorAll('.count');
    const speed = 200; // The lower the slower

    const countUp = () => {
        counters.forEach(counter => {
            const updateCount = () => {
                const target = +counter.getAttribute('data-target');
                const count = +counter.innerText;

                const inc = target / speed;

                if (count < target) {
                    counter.innerText = Math.ceil(count + inc);
                    setTimeout(updateCount, 1);
                } else {
                    counter.innerText = target;
                }
            };

            const isVisible = (elem) => {
                const bounding = elem.getBoundingClientRect();
                return (
                    bounding.top >= 0 &&
                    bounding.left >= 0 &&
                    bounding.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
                    bounding.right <= (window.innerWidth || document.documentElement.clientWidth)
                );
            };

            const onScroll = () => {
                if (isVisible(counter)) {
                    updateCount();
                    window.removeEventListener('scroll', onScroll);
                }
            };

            window.addEventListener('scroll', onScroll);
            onScroll();
        });
    };

    countUp();
});
let currentSlide = 0;
const slides = document.querySelectorAll('.slide');
const totalSlides = slides.length;

setInterval(() => {
    if (classList != undefined)
        slides[currentSlide].classList.remove('active');
    currentSlide = (currentSlide + 1) % totalSlides;
    slides[currentSlide].classList.add('active');
}, 5000);

document.addEventListener("DOMContentLoaded", function () {
    const searchResults = document.getElementById('search-results');
    const resultsList = document.getElementById('results-list');

    function performSearch() {
        const query = document.getElementById('search-bar').value.toLowerCase();
        const sideQuery = document.getElementById('side-search-bar').value.toLowerCase();
        const combinedQuery = query || sideQuery;
        resultsList.innerHTML = '';

        if (combinedQuery.trim() === '') {
            searchResults.style.display = 'none';
            return;
        }

        searchResults.style.display = 'block';
        const elements = document.querySelectorAll('body *:not(script):not(style)');
        let found = false;

        elements.forEach(element => {
            if (element.innerText.toLowerCase().includes(combinedQuery)) {
                const listItem = document.createElement('li');
                listItem.innerText = element.innerText;
                resultsList.appendChild(listItem);
                found = true;
            }
        });

        if (!found) {
            const noResultItem = document.createElement('li');
            noResultItem.innerText = 'No results found.';
            resultsList.appendChild(noResultItem);
        }
    }

    function checkEnter(event) {
        if (event.key === "Enter") {
            performSearch();
        }
    }

    document.getElementById('search-bar').addEventListener('keydown', checkEnter);
    document.getElementById('side-search-bar').addEventListener('keydown', checkEnter);
});
function checkEnter(event) {
    if (event.key === "Enter") {
        event.preventDefault();
        const query = event.target.value;
        if (query.trim()) {
            window.location.href = `search-results.html?query=${encodeURIComponent(query)}`;
        }
    }
}