// set a width and height for our SVG
        var width = {WIDTH},
            height = {HEIGHT};

        var nodes = {NODES};

        var links = {LINKS};

        //var labels = {LABELS};

        var svg = d3.select('#{GUID}').append('svg')
            .attr('width', width)
            .attr('height', height);

            svg.append('defs').append('marker')
            .attr({'id':'arrowhead',
                'viewBox':'-0 -5 100 100',
                'refX':13,
                'refY':0,
                'orient':'auto',
                'markerWidth':100,
                'markerHeight':100,
                'xoverflow':'visible'})
            .append('svg:path')
            .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
            .attr('fill', '#009')
            .style('stroke','none');
                
        var linkStyles = {LINKSTYLES}

        var link = svg.selectAll('.link')
            .data(links)
            .enter().append('line')
            .style('stroke',function(d,i) { return linkStyles[i]['StrokeHex']; })
            .style('stroke-width', function(d,i) { return linkStyles[i]['StrokeWidth']; })
            .attr('class', 'link')
            .attr('marker-end','url(#arrowhead)');

        var nodeStyles = {NODESTYLES}

        var radius = width / (nodes.length * 1);

        var node = svg.selectAll('.node')
            .data(nodes)
            .enter().append('circle')
            .attr('class', 'node')
            .style('stroke',function(d,i) { return nodeStyles[i]['StrokeHex']; })
            .style('stroke-width',function(d,i) { return nodeStyles[i]['StrokeWidth']; })
            .style('fill',function(d,i) { return nodeStyles[i]['FillHex']; })
            .attr('cx', function(d,i) { return width/2; })
            .attr('cy', function(d,i) { return height/2; })
            .attr('r', function(d,i) { return nodeStyles[i]['RadiusScale'] * radius; });

        var lbls = svg.selectAll("text")
            .data(nodes)
            .enter()
            .append('text')
            .attr('x', width/2)
            .attr('y', height/2)
            .text(function (d,i) {
                return nodeStyles[i].LabelText;})
            .each(function (d,i){
                var attrs = nodeStyles[i].LabelAttrs
                for (var j = 0; j< attrs.length; j++){
                    var attr = attrs[j];
                    d3.select(this).attr(attr.Item1,attr.Item2);
                }
            });

        function tick(e) {
                node.attr('r', function(d,i) { return nodeStyles[i]['RadiusScale'] * radius; })
                    .attr('cx', function(d) { return d.x; })
                    .attr('cy', function(d) { return d.y; })
                    .call(force.drag);

                lbls.attr('x', function(d) { return d.x + 20; })
                    .attr('y', function(d) { return d.y; })
                    .call(force.drag);

                link.attr('x1', function(d) { return d.source.x; })
                    .attr('y1', function(d) { return d.source.y; })
                    .attr('x2', function(d) { return d.source.x > d.target.x ? d.target.x - 30:d.target.x + 30 ; })
                    .attr('y2', function(d) { return d.source.y > d.target.y ? d.target.y - 30:d.target.y + 30 ; });
            }


        var force = d3.layout.force()
            .size([width, height])
            .nodes(nodes)
            .links(links)
            .gravity({GRAVITY})
            .on("tick", tick)
            .linkDistance( function (d,i) {
                return linkStyles[i]['Distance'];
            })
            .linkStrength(1)
            .charge({CHARGE})
            .start();