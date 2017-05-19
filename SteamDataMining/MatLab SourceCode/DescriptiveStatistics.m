
function theStruct = parseXML(filename)
try
    DOMnode = xmlread(filename)
catch 
    error('Failed to read XML file %s.','data.xml');    
end

try
    theStruct = parseChildNodes(DOMnode);
catch
    error('Unable to parse XML files %s.',filename);
end
end

function children = parseChildNodes(theNode)
children = [];
if theNode.hasChildNodes
    childNodes = theNode.getChildNodes;
    numChildNodes = childNodes.getLength;
    allocCell = cell(1,numChildNodes);
    
    children = struct(
    'appid', allocCell, 'name', allocCell,
    'rank', allocCell, 'owners', allocCell,
    'players', allocCell, 'price', allocCell,
    'tags', allocCell);
    
    for count = 1:numChildNodes
        theChild = childNodes.item(count-1);
        children(count) = makeStructFromNode(theChild);
    end
end
end

function nodeStruct = makeStructFromNode(theNode)

nodeStruct = struct('Name',
        
        

