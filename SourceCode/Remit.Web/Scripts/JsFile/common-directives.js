var app = angular.module('app', ['autocomplete', 'blockUI', 'ui.bootstrap', 'ngBootbox', 'mgcrea.ngStrap', 'ui.sortable', 'AngularPrint', 'multipleSelect', "ui.bootstrap.modal"]);//'naif.base64', 
app.config(function (blockUIConfig) {
    // Tell blockUI not to mark the body element as the main block scope.
    blockUIConfig.autoInjectBodyBlock = true; //work inside body tag's all content, if true. no need to mention block-ui="main" class="block-ui-main" that case 
    //blockUIConfig.blockBrowserNavigation = true;
});

app.directive('myDirec', ['$log', '$templateCache', '$compile', function ($log, $templateCache, $compile) {
    return {
        restrict: 'A',
        priority: 1000,

        link: function (scope, element, attr) {
            element.children().attr('data-toggle', 'tooltip');
            element.children().attr('data-placement', 'tooltip');
            element.children().attr('title', 'hello tool tip');

            $compile(element)(scope);
        },
    };
}]);


app.config(function ($popoverProvider) {
    angular.extend($popoverProvider.defaults, {
        html: true
    });
});

app.config(function ($datepickerProvider) {
    window.angular.extend($datepickerProvider.defaults, {
        dateFormat: 'dd-MMM-yyyy',
        startWeek: 1
    });
});





app.directive('bootstrapTooltip', function () {
    return function (scope, element, attrs) {
        attrs.$observe('title', function (title) {
            // Destroy any existing tooltips (otherwise new ones won't get initialized)
            element.tooltip('destroy');
            // Only initialize the tooltip if there's text (prevents empty tooltips)
            if (jQuery.trim(title)) element.tooltip();
        });
        element.on('$destroy', function () {
            element.tooltip('destroy');
            delete attrs.$$observers['title'];
        });
    };
});


app.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);

app.directive('tooltip', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            $(element).hover(function () {
                // on mouseenter
                $(element).tooltip('show');
            }, function () {
                // on mouseleave
                $(element).tooltip('hide');
            });
        }
    };
}); app.directive('fileUpload', function () {
    return {
        scope: true,        //create a new scope
        link: function (scope, el, attrs) {
            el.bind('change', function (event) {
                var files = event.target.files;
                //iterate files since 'multiple' may be specified on the element
                for (var i = 0; i < files.length; i++) {
                    //emit event upward
                    scope.$emit("fileSelected", { file: files[i] });
                }
            });
        }
    };
});
app.directive('validNumber', function () {
    return {
        require: '?ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            if (!ngModelCtrl) {
                return;
            }

            ngModelCtrl.$parsers.push(function (val) {
                if (angular.isUndefined(val)) {
                    var val = '';
                }
                var clean = val.replace(/[^0-9]+/g, '');
                if (val !== clean) {
                    ngModelCtrl.$setViewValue(clean);
                    ngModelCtrl.$render();
                }
                return clean;
            });

            element.bind('keypress', function (event) {
                if (event.keyCode === 32) {
                    event.preventDefault();
                }
            });
        }
    };
});

app.directive('autoComplete', function ($timeout) {
    return function (scope, iElement, iAttrs) {
        iElement.autocomplete({
            source: scope[iAttrs.uiItems],
            select: function () {
                $timeout(function () {
                    iElement.trigger('input');
                }, 0);
            }
        });
    };
});


app.directive('showErrors', function ($timeout) {
    return {
        restrict: 'A',
        require: '^form',
        link: function (scope, el, attrs, formCtrl) {
            // find the text box element, which has the 'name' attribute
            var inputEl = el[0].querySelector("[name]");
            // convert the native text box element to an angular element
            var inputNgEl = angular.element(inputEl);
            // get the name on the text box
            var inputName = inputNgEl.attr('name');

            // only apply the has-error class after the user leaves the text box
            var blurred = false;
            inputNgEl.bind('blur', function () {
                blurred = true;
                el.toggleClass('has-error', formCtrl[inputName].$invalid);
            });

            scope.$watch(function () {
                return formCtrl[inputName].$invalid
            }, function (invalid) {
                // we only want to toggle the has-error class after the blur
                // event or if the control becomes valid
                if (!blurred && invalid) { return }
                el.toggleClass('has-error', invalid);
            });

            scope.$on('show-errors-check-validity', function () {
                el.toggleClass('has-error', formCtrl[inputName].$invalid);
            });

            scope.$on('show-errors-reset', function () {
                $timeout(function () {
                    el.removeClass('has-error');
                }, 0, false);
            });
        }
    }
});
app.directive("passwordVerify", function () {
    return {
        require: "ngModel",
        scope: {
            passwordVerify: '='
        },
        link: function (scope, element, attrs, ctrl) {
            scope.$watch(function () {
                var combined;

                if (scope.passwordVerify || ctrl.$viewValue) {
                    combined = scope.passwordVerify + '_' + ctrl.$viewValue;
                }
                return combined;
            }, function (value) {
                if (value) {
                    ctrl.$parsers.unshift(function (viewValue) {
                        var origin = scope.passwordVerify;
                        if (origin !== viewValue) {
                            ctrl.$setValidity("passwordVerify", false);
                            return undefined;
                        } else {
                            ctrl.$setValidity("passwordVerify", true);
                            return viewValue;
                        }
                    });
                }
            });
        }
    };
});
app.directive('modal', function () {
    return {
        template: '<div class="modal fade">' +
            '<div class="modal-dialog">' +
              '<div class="modal-content">' +
                '<div class="modal-header">' +
                  '<button type="button" class="close" style="color:white;font-size:22px;border:1px solid #000; padding:2px 6px;border-radius:50px;background-color:#000;" title="Close"  data-dismiss="modal" aria-hidden="true">&times;</button>' +
                  '<h4 class="modal-title">{{ title }}</h4>' +
                '</div>' +
                '<div class="modal-body" ng-transclude></div>' +
                '</div>' +
            '</div>' +
          '</div>',
        restrict: 'E',
        transclude: true,
        replace: true,
        scope: true,
        link: function postLink(scope, element, attrs) {
            scope.title = attrs.title;
            scope.$watch(attrs.visible, function (value) {
                if (value == true)
                    $(element).modal('show');
                else
                    $(element).modal('hide');
            });

            $(element).on('shown.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = true;
                });
            });

            $(element).on('hidden.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = false;
                });
            });
        }
    };
});

app.directive('modala', function () {
    return {
        template: '<div class="modal fade" data-backdrop="static">' +
            '<div class="modal-dialog">' +
              '<div class="modal-content">' +
                '<div class="modal-header">' +
                  '<button type="button" class="close" style="visibility:hidden" title="Close"  data-dismiss="modal" aria-hidden="true">&times;</button>' +
                  '<h4 class="modal-title">{{ title }}</h4>' +
                '</div>' +
                '<div class="modal-body" ng-transclude></div>' +
                '</div>' +
            '</div>' +
          '</div>',
        restrict: 'E',
        transclude: true,
        replace: true,
        scope: true,
        link: function postLink(scope, element, attrs) {
            scope.title = attrs.title;
            scope.$watch(attrs.visible, function (value) {
                if (value == true)
                    $(element).modal('show');
                else
                    $(element).modal('hide');
            });

            $(element).on('shown.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = true;
                });
            });

            $(element).on('hidden.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = false;
                });
            });
        }
    };
});


app.directive('focusOn', function () {
    return function (scope, elem, attr) {
        scope.$on('focusOn', function (e, name) {
            if (name === attr.focusOn) {
                elem[0].focus();
            }
        });
    };
});
app.directive('ngReallyClick', [function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.bind('click', function () {
                var message = attrs.ngReallyMessage;
                if (message && confirm(message)) {
                    scope.$apply(attrs.ngReallyClick);
                }
            });
        }
    }
}]);

app.directive('autoCompleteDirective', function ($http, $timeout) {
    return {
        restrict: 'A',
        scope: {
            url: '@',
            paramlist: '@',
            ngModel: '='
        },
        require: 'ngModel',
        link: function (scope, elm, attrs, ngModel) {
            elm.autocomplete({
                source: function (req, res) {
                    var paramsIn = [];
                    if (scope.paramlist != undefined && scope.paramlist != '') paramsIn = JSON.parse(scope.paramlist)[0];
                    var paramsTemp = Object.assign({}, paramsIn, ({ id: req.term }));

                    $http({ url: scope.url, params: paramsTemp }).success(function (data) {
                        $timeout(function () { res(data); }, 300);

                    }).error(function (a, b, c) {
                        alert(a);
                    });
                },
                minLength: 1,
                select: function (event, ui) {
                    //ngModel.$getViewValue(ui.item.value);
                    scope.$apply(function () {
                        scope.ngModel = ui.item.value;
                    });
                }
            })
        }

    }
});

