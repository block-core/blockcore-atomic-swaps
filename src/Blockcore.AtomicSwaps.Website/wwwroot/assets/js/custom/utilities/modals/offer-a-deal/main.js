"use strict";

// Class definition
var DEXModalOfferADeal = function () {
    // Private variables
	var stepper;
	var stepperObj;
	var form;	

	// Private functions
	var initStepper = function () {
		// Initialize Stepper
		stepperObj = new DEXStepper(stepper);
	}

	return {
		// Public functions
		init: function () {
			stepper = document.querySelector('#dex_modal_offer_a_deal_stepper');
			form = document.querySelector('#dex_modal_offer_a_deal_form');

			initStepper();
		},

		getStepper: function () {
			return stepper;
		},

		getStepperObj: function () {
			return stepperObj;
		},
		
		getForm: function () {
			return form;
		}
	};
}();


    DEXModalOfferADeal.init();
    DEXModalOfferADealType.init();
    DEXModalOfferADealDetails.init();
    DEXModalOfferADealFinance.init();
    DEXModalOfferADealComplete.init();
});

// Webpack support
if (typeof module !== 'undefined' && typeof module.exports !== 'undefined') {
	window.DEXModalOfferADeal = module.exports = DEXModalOfferADeal;
}