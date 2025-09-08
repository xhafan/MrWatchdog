const path = require('path');
const fs = require('fs');
const esbuild = require("esbuild");

const options = {
  entryPoints: ["Features/Shared/site.ts"],
  bundle: true,
  minify: true,
  sourcemap: true,
  sourcesContent: true,  // embed TS content into the map
  outfile: "wwwroot/assets/bundle.js",
  target: ["es2022"],
  absWorkingDir: process.cwd(),
  plugins: [{
    name: 'normalize-sources', // remove initial "../" from source paths so stacktrace-js can find the files when mapping ts stack trace for errors
    setup(build) {
      build.onEnd(result => {
        const mapPath = path.resolve(build.initialOptions.outfile + '.map'); // e.g., bundle.js.map
        if (fs.existsSync(mapPath)) {
            const map = JSON.parse(fs.readFileSync(mapPath, 'utf8'));
            map.sources = map.sources.map(s => s.replace(/^\.\.\//, ''));
            fs.writeFileSync(mapPath, JSON.stringify(map));
            console.log(new Date().toISOString(), 'Normalized source map paths in', mapPath);
        }
      });
    }
  }]
};

async function run() {
  if (process.argv.includes("--watch")) {
    const ctx = await esbuild.context(options);
    await ctx.watch();
    console.log("👀 Watching for changes...");
  } else {
    await esbuild.build(options);
    console.log("✅ Build complete");
  }
}

run().catch(() => process.exit(1));