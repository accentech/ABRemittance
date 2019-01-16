/**
 * angular-strap
 * @version v2.3.2 - 2015-09-15
 * @link http://mgcrea.github.io/angular-strap
 * @author Olivier Louvignes <olivier@mg-crea.com> (https://github.com/mgcrea)
 * @license MIT License, http://www.opensource.org/licenses/MIT
 */
(function (window, document, undefined) {
  'use strict';
 

  angular.module('mgcrea.ngStrap').run(['$templateCache', function ($templateCache) {
    $templateCache.put('popover/docs/popover-content.custom.tpl.html', '<form name="popoverForm"><p ng-bind-html="content" style="min-width:300px"></p><pre>2 + 3 = <span ng-cloak>{{ 2 + 3 }}</span></pre><div class="form-group"><div class="input-group"><div class="input-group-addon">@</div><input class="form-control" type="email" placeholder="Enter email"></div></div><div class="form-actions"><button type="button" class="btn btn-danger" ng-click="$hide()">Close</button> <button type="button" class="btn btn-primary" ng-click="popover.saved=true;$hide()">Save changes</button></div></form>');
  }]);
 
})(window, document);