"use strict";

// Class definition
var DEXPricingGeneral = function () {
    // Private variables
    var element;
	var planPeriodMonthButton;
	var planPeriodAnnualButton;

	var changePlanPrices = function(type) {
		var items = [].slice.call(element.querySelectorAll('[data-dex-plan-price-month]'));

		items.map(function (item) {
			var monthPrice = item.getAttribute('data-dex-plan-price-month');
			var annualPrice = item.getAttribute('data-dex-plan-price-annual');

			if ( type === 'month' ) {
				item.innerHTML = monthPrice;
			} else if ( type === 'annual' ) {
				item.innerHTML = annualPrice;
			}
		});
	}

    var handlePlanPeriodSelection = function(e) {

        // Handle period change
        planPeriodMonthButton.addEventListener('click', function (e) {
            e.preventDefault();

            planPeriodMonthButton.classList.add('active');
            planPeriodAnnualButton.classList.remove('active');

            changePlanPrices('month');
        });

		planPeriodAnnualButton.addEventListener('click', function (e) {
            e.preventDefault();

            planPeriodMonthButton.classList.remove('active');
            planPeriodAnnualButton.classList.add('active');
            
            changePlanPrices('annual');
        });
    }

    // Public methods
    return {
        init: function () {
            element = document.querySelector('#dex_pricing');
			planPeriodMonthButton = element.querySelector('[data-dex-plan="month"]');
			planPeriodAnnualButton = element.querySelector('[data-dex-plan="annual"]');

            // Handlers
            handlePlanPeriodSelection();
        }
    }
}();

