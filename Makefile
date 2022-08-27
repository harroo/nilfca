
output:
	mcs -recurse:source/*.cs -r:lib/DSharpPlus.dll -r:lib/Newtonsoft.Json.dll -out:build/nilfca.cil

run:
	mono build/nilfca.cil

ready:
	echo "getting ready for build"
	mkdir build
	cp -v lib/* build

example:
	mcs client-example.cs -out:client-example.cil
