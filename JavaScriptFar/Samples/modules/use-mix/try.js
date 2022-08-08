
function test() {
    let name1 = CommonJS.myName
    let name2 = Standard.myName
    far.Message(`${name1}\n${name2}`)
}

test()
