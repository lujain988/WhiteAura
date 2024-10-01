$(document).ready(function() {
    // Example user data
    const user = JSON.parse(localStorage.getItem('user')) || {
      name: 'John Doe',
      email: 'john.doe@example.com',
      phone: '+1234567890',
      profilePicture: 'pic/profile-placeholder.png',
      bookings: [
        {
          venue: 'Al Hamra Palace Halls',
          date: 'June 20, 2024',
          price: '$1000',
          city: 'Amman'
        },
        {
          venue: 'Al Numan Halls',
          date: 'May 15, 2024',
          price: '$1200',
          city: 'Irbid'
        }
      ]
    };

    // Set user data
    $('#profile-name').text(user.name);
    $('#profile-fullname').text(user.name);
    $('#profile-email').text(user.email);
    $('#profile-email-detail').text(user.email);
    $('#profile-phone').text(user.phone);
    $('#profile-picture').attr('src', user.profilePicture);

    // Add bookings to the booking list
    const bookingList = $('#booking-list');
    user.bookings.forEach(booking => {
      const bookingItem = `
        <li>
          <strong>Venue:</strong> ${booking.venue}<br>
          <strong>Date:</strong> ${booking.date}<br>
          <strong>Price:</strong> ${booking.price}<br>
          <strong>City:</strong> ${booking.city}
        </li>
      `;
      bookingList.append(bookingItem);
    });
  });

  function editProfile() {
    const user = JSON.parse(localStorage.getItem('user')) || {
      name: 'John Doe',
      email: 'john.doe@example.com',
      phone: '+1234567890',
      profilePicture: 'pic/profile-placeholder.png'
    };
    $('#editName').val(user.name);
    $('#editEmail').val(user.email);
    $('#editPhone').val(user.phone);
    $('#editProfilePicture').val('');
    $('#editProfileModal').modal('show');
  }

  $('#editProfileForm').on('submit', function(e) {
    e.preventDefault();
    const user = JSON.parse(localStorage.getItem('user')) || {
      name: 'John Doe',
      email: 'john.doe@example.com',
      phone: '+1234567890',
      profilePicture: 'pic/profile-placeholder.png'
    };
    user.name = $('#editName').val();
    user.email = $('#editEmail').val();
    user.phone = $('#editPhone').val();

    const profilePictureInput = $('#editProfilePicture')[0];
    if (profilePictureInput.files && profilePictureInput.files[0]) {
      const reader = new FileReader();
      reader.onload = function(e) {
        user.profilePicture = e.target.result;
        $('#profile-picture').attr('src', user.profilePicture);
        localStorage.setItem('user', JSON.stringify(user));
      }
      reader.readAsDataURL(profilePictureInput.files[0]);
    } else {
      localStorage.setItem('user', JSON.stringify(user));
    }

    $('#profile-name').text(user.name);
    $('#profile-fullname').text(user.name);
    $('#profile-email').text(user.email);
    $('#profile-email-detail').text(user.email);
    $('#profile-phone').text(user.phone);
    $('#editProfileModal').modal('hide');
  });