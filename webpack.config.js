var path = require("path");
var webpack = require("webpack");
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var HtmlWebpackPlugin = require("html-webpack-plugin");
var isProduction = (process.env.NODE_ENV === 'production');

var cfg = {
  entry: "UI.js",
  resolve: {
    extensions: ["", ".js", ".scss"],
    root: [__dirname, path.join(__dirname, "static"), path.join(__dirname, "fable-out")]
  },
  output: {
    path: path.join(__dirname, "dist"),
    filename: "app.js"
  },
  module: {
    loaders: [],
    preLoaders: []
  },
  plugins: [
    new webpack.DefinePlugin({
      'process.env': {
        'NODE_ENV': process.env.NODE_ENV
      }
    }),
    new HtmlWebpackPlugin({
      filename: "index.html",
      template: path.join(__dirname, "static", "index.ejs")
    })
  ]
};

if (isProduction) {
  cfg.plugins = cfg.plugins.concat([
    new ExtractTextPlugin("app.css")
  ]);

  cfg.module.loaders = [{
    test: /\.scss$/,
    exclude: /node_modules/,
    loader: ExtractTextPlugin.extract(["css", "sass"])
  }];
} else {
  cfg.module.loaders = [{
    test: /\.scss$/,
    loaders: ["style", "css?sourceMap", "sass?sourceMap"]
  }];

  cfg.module.preLoaders = [{
    test: /\.js$/,
    exclude: /node_modules/,
    loader: "source-map-loader"
  }];

  cfg.devtool = "source-map";
}

module.exports = cfg;