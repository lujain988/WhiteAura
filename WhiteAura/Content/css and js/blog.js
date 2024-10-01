$(document).ready(function() {
    const params = new URLSearchParams(window.location.search);
    const title = params.get('title');

    // Blog data
    const blogs = {
      "Magical Night at Al Hamra Palace Halls": {
        date: "June 20, 2024",
        content: `
          <img src="pic/v2.jpg" alt="Al Hamra Palace Halls">
          <p>Witness the magic of a stunning wedding held at Al Hamra Palace Halls in Amman. The night was filled with elegance, joy, and unforgettable moments. The venue's luxurious decor and top-notch facilities provided the perfect backdrop for a truly magical evening.</p>
          <p>From the grand entrance to the beautifully decorated hall, every detail was meticulously planned to create an atmosphere of enchantment. Guests were treated to a delightful dinner, followed by dancing and celebration late into the night.</p>
          <p>This wedding at Al Hamra Palace Halls was a night to remember, filled with love, laughter, and cherished memories.</p>
          <p>Our team worked closely with the couple to ensure every aspect of their special day was perfect. From the floral arrangements to the lighting, every detail was tailored to reflect their unique style and preferences. The result was a breathtaking event that left a lasting impression on everyone who attended.</p>
        `,
        gallery: ["pic/v2.jpg", "pic/f2.jpg", "pic/pl7.jpeg", "pic/black1.jpeg"]
      },
      "Elegance at Al Numan Halls": {
        date: "May 15, 2024",
        content: `
          <img src="pic/f2.jpg" alt="Al Numan Halls">
          <p>Discover the elegance and sophistication of a wedding held at Al Numan Halls in Irbid. Every detail was meticulously planned to create a perfect day. The spacious halls, elegant decor, and modern amenities made Al Numan Halls the ideal venue for this beautiful celebration.</p>
          <p>Guests enjoyed a wonderful dinner and entertainment, creating memories that will last a lifetime. The couple's vision was brought to life with stunning floral arrangements, elegant table settings, and a warm, inviting atmosphere.</p>
          <p>The evening was filled with heartfelt speeches, joyful laughter, and plenty of dancing. It was a celebration of love and happiness, set against the backdrop of Al Numan Halls' timeless elegance.</p>
          <p>Our team took great care to ensure that every aspect of the wedding was perfect. From the initial planning stages to the final moments of the event, we were dedicated to making the couple's dream wedding a reality. The result was a day filled with unforgettable moments and cherished memories.</p>
        `,
        gallery: ["pic/f2.jpg", "pic/pl7.jpeg", "pic/black1.jpeg", "pic/service3.jpg"]
      },
      "Rustic Charm at Al Danah Farm": {
        date: "April 10, 2024",
        content: `
          <img src="pic/pl7.jpeg" alt="Al Danah Farm">
          <p>Experience the rustic charm and beauty of a wedding held at Al Danah Farm in Zarqa. This outdoor celebration was a perfect blend of nature and elegance. The lush gardens and serene environment provided a picturesque setting for the wedding ceremony and reception.</p>
          <p>Guests were treated to an unforgettable evening under the stars, surrounded by the natural beauty of the farm. The couple's love for nature was reflected in every detail, from the rustic decor to the locally-sourced menu.</p>
          <p>The ceremony took place in a beautiful garden, with the sound of birds singing and a gentle breeze adding to the romantic atmosphere. The reception was held in a charming barn, decorated with fairy lights and rustic touches that created a warm, inviting ambiance.</p>
          <p>Our team worked closely with the couple to create a wedding that was both beautiful and meaningful. Every detail was carefully planned to reflect their love for each other and their connection to nature. The result was a truly magical event that left a lasting impression on everyone who attended.</p>
        `,
        gallery: ["pic/pl7.jpeg", "pic/black1.jpeg", "pic/service3.jpg", "pic/wed.jpg"]
      },
      "Grand Celebration at Al Roubi Palace": {
        date: "March 5, 2024",
        content: `
          <img src="pic/black1.jpeg" alt="Al Roubi Palace for Celebrations">
          <p>Celebrate a grand wedding at Al Roubi Palace for Celebrations in Mafraq. This event was filled with traditional charm and modern elegance. The grand halls and exquisite decor created a perfect backdrop for the wedding ceremony and reception.</p>
          <p>Guests enjoyed a night of celebration, dancing, and unforgettable moments. The couple's vision was brought to life with stunning floral arrangements, elegant table settings, and a warm, inviting atmosphere.</p>
          <p>The evening was filled with heartfelt speeches, joyful laughter, and plenty of dancing. It was a celebration of love and happiness, set against the backdrop of Al Roubi Palace's timeless elegance.</p>
          <p>Our team took great care to ensure that every aspect of the wedding was perfect. From the initial planning stages to the final moments of the event, we were dedicated to making the couple's dream wedding a reality. The result was a day filled with unforgettable moments and cherished memories.</p>
        `,
        gallery: ["pic/black1.jpeg", "pic/service3.jpg", "pic/wed.jpg", "pic/v2.jpg"]
      },
      "Joyful Moments at Al Shaimaa Hall": {
        date: "February 28, 2024",
        content: `
          <img src="pic/service3.jpg" alt="Al Shaimaa Hall for Celebrations">
          <p>Relive the joyful moments of a wedding held at Al Shaimaa Hall for Celebrations in Aqaba. The hall provided a perfect backdrop for a memorable event. The spacious and elegantly decorated hall was the perfect setting for this joyous celebration.</p>
          <p>Guests were treated to an evening of wonderful food, music, and dancing. The couple's love and happiness were evident in every detail, from the beautiful floral arrangements to the heartfelt speeches and toasts.</p>
          <p>The evening was filled with laughter, joy, and plenty of dancing. It was a celebration of love and happiness, set against the backdrop of Al Shaimaa Hall's timeless elegance.</p>
          <p>Our team worked closely with the couple to create a wedding that was both beautiful and meaningful. Every detail was carefully planned to reflect their love for each other and their connection to their families. The result was a truly magical event that left a lasting impression on everyone who attended.</p>
        `,
        gallery: ["pic/service3.jpg", "pic/wed.jpg", "pic/v2.jpg", "pic/f2.jpg"]
      },
      "Luxurious Wedding at Adonis Halls": {
        date: "January 22, 2024",
        content: `
          <img src="pic/wed.jpg" alt="Adonis Halls">
          <p>Enjoy the luxury and grandeur of a wedding held at Adonis Halls in Irbid. This event was a true reflection of opulence and sophistication. The grand halls and exquisite decor created a perfect backdrop for the wedding ceremony and reception.</p>
          <p>Guests enjoyed a night of celebration, dancing, and unforgettable moments. The couple's vision was brought to life with stunning floral arrangements, elegant table settings, and a warm, inviting atmosphere.</p>
          <p>The evening was filled with heartfelt speeches, joyful laughter, and plenty of dancing. It was a celebration of love and happiness, set against the backdrop of Adonis Halls' timeless elegance.</p>
          <p>Our team took great care to ensure that every aspect of the wedding was perfect. From the initial planning stages to the final moments of the event, we were dedicated to making the couple's dream wedding a reality. The result was a day filled with unforgettable moments and cherished memories.</p>
        `,
        gallery: ["pic/wed.jpg", "pic/v2.jpg", "pic/f2.jpg", "pic/pl7.jpeg"]
      }
    };

    const blog = blogs[title];

    if (blog) {
      $('#blog-title').text(title);
      $('#blog-date').text(blog.date);
      $('#blog-content').html(blog.content);

      const galleryContainer = $('#blog-gallery');
      blog.gallery.forEach(image => {
        galleryContainer.append(`<img src="${image}" alt="Gallery Image">`);
      });

      $('#blog-gallery img').click(function() {
        const src = $(this).attr('src');
        $('#overlay-image').attr('src', src);
        $('#overlay').show();
      });

      $('#close-overlay').click(function() {
        $('#overlay').hide();
      });
    } else {
      $('#blog-title').text('Blog Not Found');
      $('#blog-date').hide();
      $('#blog-content').html('<p>Sorry, the blog post you are looking for does not exist.</p>');
    }
  });