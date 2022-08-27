
output:
	mcs -recurse:source/*.cs -out:build/nilfca.cil

run:
	mono build/nilfca.cil

ready:
	echo "getting ready for build"
	mkdir build
