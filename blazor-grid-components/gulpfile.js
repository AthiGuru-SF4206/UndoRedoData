var fs = require('fs');
var gulp = require('gulp');
require('require-dir')('build');  // auto-loads all files in build/ including compile-bundle.js
module.exports = gulp;
if(fs.existsSync('./node_modules/@syncfusion/ej2-staging')){
    const { publishReportforSources } = require("@syncfusion/ej2-staging/src/mail");
    require("@syncfusion/ej2-staging/src/blazor-publish");
    require('require-dir')('node_modules/@syncfusion/ej2-staging/src/blazor-component-repo');  
    gulp.task("publish-report", publishReportforSources);    
}
