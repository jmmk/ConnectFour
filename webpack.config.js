var path = require("path");
var webpack = require("webpack");
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var HtmlWebpackPlugin = require("html-webpack-plugin");
var isProduction = (process.env.NODE_ENV === 'production');

var cfg = {
  entry: "UI.js",
  resolve: {
    extensions: [".js", ".scss"],
    modules: [path.join(__dirname, "static"), path.join(__dirname, "fable-out"), path.join(__dirname, "node_modules")]
  },
  output: {
    path: path.join(__dirname, "dist"),
    filename: "app.js"
  },
  module: {
    rules: [{
      test: /\.js$/,
      loader: "babel-loader",
      exclude: /node_modules/,
      options: {
        "presets": [
          ["es2015", { "modules": false }]
        ]
      }
    }],
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
  cfg.plugins.push( new ExtractTextPlugin({ filename: "app.css" }) );

  cfg.module.rules.push({
    test: /\.scss$/,
    exclude: /node_modules/,
    loader: ExtractTextPlugin.extract(["css-loader", "sass-loader"])
  });
} else {
  cfg.module.rules.push({
    test: /\.scss$/,
    use: ["style-loader", "css-loader?sourceMap", "sass-loader?sourceMap"]
  });

  cfg.module.rules.push({
    test: /\.js$/,
    enforce: "pre",
    exclude: /node_modules/,
    loader: "source-map-loader"
  });

  cfg.devtool = "source-map";
}

module.exports = cfg;