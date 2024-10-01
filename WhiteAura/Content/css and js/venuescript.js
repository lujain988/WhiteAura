AOS.init();

function setVenueInfo(venue, price) {
    localStorage.setItem('selectedPlace', venue);
    localStorage.setItem('selectedPrice', price);
}

function searchVenues(event) {
  event.preventDefault();
  const city = document.getElementById('filterCity').value.toLowerCase();
  const price = parseInt(document.getElementById('price_range').value, 10);
  const type = document.getElementById('type_of_place').value.toLowerCase();
  const venueCards = document.querySelectorAll('.packages_box');
  
  venueCards.forEach(card => {
    const cardCity = card.getAttribute('data-city').toLowerCase();
    const cardPrice = parseInt(card.getAttribute('data-price'), 10);
    const cardType = card.getAttribute('data-type').toLowerCase();

    if ((city === 'all' || cardCity === city) && cardPrice <= price && (type === 'all' || cardType === type)) {
      card.parentElement.style.display = 'block';
    } else {
      card.parentElement.style.display = 'none';
    }
  });
}

function setVenueDetails(event) {
  event.preventDefault();
  const card = event.target.closest('.packages_box');
  const venue = card.querySelector('h3').textContent;
  const description = card.querySelector('p').textContent;
  const city = card.getAttribute('data-city');
  const price = card.getAttribute('data-price');
  const type = card.getAttribute('data-type');
  const guests = card.getAttribute('data-guests');
  const location = card.getAttribute('data-location');
  const album = card.getAttribute('data-album');
  const photo = card.querySelector('img').getAttribute('src');

  const queryParams = new URLSearchParams({
    venue,
    description,
    city,
    price,
    type,
    guests,
    location,
    album,
    photo
  });

  window.location.href = `venue-details.html?${queryParams.toString()}`;
}
