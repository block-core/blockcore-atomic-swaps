"use strict";

// Class definition
var DEXModalOfferADealComplete = function () {
	// Variables
	var startButton;
	var form;
	var stepper;

	// Private functions
	var handleForm = function() {
		startButton.addEventListener('click', function () {
			stepper.goTo(1);
		});
	}

	return {
		// Public functions
		init: function () {
			form = DEXModalOfferADeal.getForm();
			stepper = DEXModalOfferADeal.getStepperObj();
			startButton = DEXModalOfferADeal.getStepper().querySelector('[data-dex-element="complete-start"]');

			handleForm();
		}
	};
}();

// Webpack support
if (typeof module !== 'undefined' && typeof module.exports !== 'undefined') {
	window.DEXModalOfferADealComplete = module.exports = DEXModalOfferADealComplete;
}