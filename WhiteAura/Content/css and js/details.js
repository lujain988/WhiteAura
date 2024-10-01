$(document).ready(function() {
    const params = new URLSearchParams(window.location.search);
    const venue = params.get('venue');
    const description = params.get('description');
    const city = params.get('city');
    const price = params.get('price');
    const type = params.get('type');
    const guests = params.get('guests');
    const location = params.get('location');
    const photo = params.get('photo');

    $('#venue-name').text(venue);
    $('#venue-description').text(description);
    $('#venue-city').text(city);
    $('#venue-price').text(price);
    $('#venue-type').text(type);
    $('#venue-guests').text(guests);
    $('#venue-location').text(location);
    $('#venue-photo').attr('src', photo);

    const venueImages = {
      'Al Hamra Palace Halls': [
        'pic/f3.png',
        'pic/f2.jpg',
        'pic/pl7.jpeg',
        'pic/black1.jpeg',
        'pic/service3.jpg',
        'pic/wed.jpg',
        'pic/service1.jpg'
      ],
      'Al Numan Halls': [
        'pic/v1.jpg',
        'pic/f2.jpg',
        'pic/pl7.jpeg',
        'pic/black1.jpeg',
        'pic/service3.jpg',
        'pic/wed.jpg',
        'pic/service1.jpg'
      ],
      'Al Danah Farm': [
        'pic/f1.jpg',
        'pic/f2.jpg',
        'pic/pl7.jpeg',
        'pic/black1.jpeg',
        'pic/service3.jpg',
        'pic/wed.jpg',
        'pic/service1.jpg'
      ],
      'Al Roubi Palace for Celebrations': [
        'pic/v3.jpg',
        'pic/f2.jpg',
        'pic/pl7.jpeg',
        'pic/black1.jpeg',
        'pic/service3.jpg',
        'pic/wed.jpg',
        'pic/service1.jpg'
      ],
      'Al Shaimaa Hall For Celebrations': [
        'pic/f2.jpg',
        'pic/f2.jpg',
        'pic/pl7.jpeg',
        'pic/black1.jpeg',
        'pic/service3.jpg',
        'pic/wed.jpg',
        'pic/service1.jpg'
      ],
      'Adonis Halls': [
        'pic/f3.png',
        'pic/f2.jpg',
        'pic/pl7.jpeg',
        'pic/black1.jpeg',
        'pic/service3.jpg',
        'pic/wed.jpg',
        'pic/service1.jpg'
      ]
    };

    const album = venueImages[venue] || [];

    const albumContainer = $('#venue-album');
    album.forEach((image, index) => {
      const projectClass = `project${index + 1}`;
      const projectElement = `
        <div class="project ${projectClass}">
          <img class="smallImage" src="${image}" alt="Venue Image ${index + 1}">
          <div class="overlay">
            <div class="overlay-inner">
              <button class="close">Close X</button>
              <div class="hdImgs">
                <img class="squareImg" src="${image}" alt="Venue Image ${index + 1}">
              </div>
            </div>
          </div>
          <div class="btn-box">
            <button class="btn">View</button>
          </div>
        </div>
      `;
      albumContainer.append(projectElement);
    });

    $(`[unique-script-id="w-w-dm-id"] .btn-box`).click(function() {
      $(this).parent().children(".overlay").show();
    });

    $(`[unique-script-id="w-w-dm-id"] .close`).click(function() {
      $(".overlay").hide();
    });
  });