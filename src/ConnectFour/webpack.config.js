var path = require("path");
var webpack = require("webpack");

var cfg = {
  devtool: "source-map",
  entry: "./out/UI.js",
  output: {
    path: path.join(__dirname, "../.."),
    filename: "app.js"
  },
  module: {
    loaders: [
      {
        include: path.join(__dirname, 'css/app.scss'),
        loaders: ["style", "css", "sass"]
      }
    ],
    preLoaders: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        loader: "source-map-loader"
      }
    ]
  }
};

module.exports = cfg;
