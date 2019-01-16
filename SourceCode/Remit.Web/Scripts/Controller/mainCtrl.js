(function () {
    'use strict';

    angular.module('ModalDemoApp')
        .controller('MainCtrl', ['$modal', function ($modal) {
            var vm = this;

            vm.open = function () {
                var modalInstance = $modal.open({
                    templateUrl: 'app/views/partials/modalView.html',
                    controller: 'ModalCtrl as vm'
                });
            };

        }]);

}());