"use strict";

// Class definition
var DEXModalCreateProjectComplete = function () {
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
			form = DEXModalCreateProject.getForm();
			stepper = DEXModalCreateProject.getStepperObj();
			startButton = DEXModalCreateProject.getStepper().querySelector('[data-dex-element="complete-start"]');

			handleForm();
		}
	};
}();

// Webpack support
if (typeof module !== 'undefined' && typeof module.exports !== 'undefined') {
	window.DEXModalCreateProjectComplete = module.exports = DEXModalCreateProjectComplete;
}
