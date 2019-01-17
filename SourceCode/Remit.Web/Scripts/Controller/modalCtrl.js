(function () {
    'use strict';

    angular.module('ModalDemoApp')
        .controller('ModalCtrl', ['$modalInstance', function ($modalInstance) {
            var vm = this;

            vm.close = function () {
                $modalInstance.close();
            };

        }]);

}());