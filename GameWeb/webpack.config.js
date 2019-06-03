var path = require('path');

module.exports = {
    mode: 'development',
    entry: './Components/index.js',
    output: {
        filename: './wwwroot/js/[name].bundle.js',
        path: path.resolve(__dirname, '')
    },
    mode: process.env.NODE_ENV === 'production' ? 'production' : 'development',
	module: {
		rules: [
			{
				test: /\.jsx?$/,
				exclude: /node_modules/,
				loader: 'babel-loader',
			},
		],
	},
    resolve: {
        extensions: ['.js', '.jsx'],
    },
};