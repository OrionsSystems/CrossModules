const path = require('path');

module.exports = {
    mode: 'development',
    //mode: 'production',
    watch: true,
    entry: './index.js',
    output: {
        filename: '../../wwwroot/Orions.Crossmodules.Desi.js',
        path: path.resolve(__dirname, 'dist'),
    },
    module: {
        rules: [
            {
                test: /\.m?js$/,
                exclude: /(node_modules|bower_components)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env']
                    }
                }
            }
        ]
    }
};
