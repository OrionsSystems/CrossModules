const path = require('path');

module.exports = {
    mode: 'development',
    entry: './index.js',
    output: {
        filename: '../../wwwroot/Orions.Crossmodules.Desi.js',
        path: path.resolve(__dirname, 'dist'),
    }
};
