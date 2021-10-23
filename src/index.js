const fs = require("fs");

const json = require("./output.json");

const sorted = json.sort((a, b) => {
    return b.combo - a.combo;
});

fs.writeFileSync("./sorted.json", JSON.stringify(sorted, null, 4));
