"use strict";

// Class definition
var DEXModalCreateProject = function () {
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
			stepper = document.querySelector('#dex_modal_create_project_stepper');
			form = document.querySelector('#dex_modal_create_project_form');

			initStepper();
		},

		getStepperObj: function () {
			return stepperObj;
		},

		getStepper: function () {
			return stepper;
		},
		
		getForm: function () {
			return form;
		}
	};
}();


	DEXModalCreateProject.init();
	DEXModalCreateProjectType.init();
	DEXModalCreateProjectBudget.init();
	DEXModalCreateProjectSettings.init();
	DEXModalCreateProjectTeam.init();
	DEXModalCreateProjectTargets.init();
	DEXModalCreateProjectFiles.init();
	DEXModalCreateProjectComplete.init();
});

// Webpack support
if (typeof module !== 'undefined' && typeof module.exports !== 'undefined') {
	window.DEXModalCreateProject = module.exports = DEXModalCreateProject;
}
