const path = require('path');

module.exports = {
    //mode: 'development',
    mode: 'production',
    entry: './index.js',
    output: {
        filename: 'Orions.Crossmodules.ComponentsBundle.js',
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
