"use strict";

// Class definition
var DEXProfileFollowers = function () {
    // init variables
    var showMoreButton = document.getElementById('dex_followers_show_more_button');
    var showMoreCards = document.getElementById('dex_followers_show_more_cards');

    // Private functions
    var handleShowMore = function () {
        // Show more click
        showMoreButton.addEventListener('click', function (e) {
            showMoreButton.setAttribute('data-dex-indicator', 'on');

            // Disable button to avoid multiple click 
            showMoreButton.disabled = true;
            
            setTimeout(function() {
                // Hide loading indication
                showMoreButton.removeAttribute('data-dex-indicator');

                // Enable button
				showMoreButton.disabled = false;

                // Hide button
                showMoreButton.classList.add('d-none');

                // Show card
                showMoreCards.classList.remove('d-none');

                // Scroll to card
                DEXUtil.scrollTo(showMoreCards, 200);
            }, 2000);
        });
    }

    // Public methods
    return {
        init: function () {
            handleShowMore();
        }
    }
}();

