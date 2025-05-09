const esbuild = require("esbuild");

const options = {
  entryPoints: ["Pages/Shared/site.ts"],
  bundle: true,
  minify: true,
  sourcemap: true,
  outfile: "wwwroot/bundle.js",
  target: ["es2022"]
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