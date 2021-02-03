const dirTree = require("directory-tree")
const crypto = require('crypto')
const fs = require('fs')

let manifest = {
	version: "1.0.0",
	files: {}
}
dirTree("./fb_003", undefined, (item, path, stats) =>
{
	let fname = item.path.replace(/^fb_003\//, '')
	let fcont = fs.readFileSync(item.path).toString('utf8')
	manifest.files[fname] = {
		"hash": crypto.createHash('sha256').update(fcont).digest('hex'),
		"size": stats.size
	}
})

fs.writeFileSync("./fb_003/manifest.json", JSON.stringify(manifest))