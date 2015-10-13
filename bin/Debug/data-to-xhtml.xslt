<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:xs="http://www.w3.org/2001/XMLSchema" 
	xmlns:fn="http://www.w3.org/2005/xpath-functions"
	xmlns:rb="http://www.brotherus.net/xslt">
	<xsl:output method="xhtml" version="1.0" encoding="UTF-8" indent="yes"/>

	<xsl:include href="common.xslt"/>
	
	<xsl:template match="/">
		<html>
			<head>
				<title>Good Time Tracker - Time Usage Report</title>
			</head>
			<body>
				<h1>Good Time Tracker - Work Time Usage Report</h1>
				<xsl:variable name="workActivities" select="//Activity[Type = 'Work']"/>
				<xsl:variable name="days" select="for $time in $workActivities/StartTime return rb:date-from-dateTime($time)"/>
				<xsl:variable name="distinctDays" select="distinct-values($days)"/>
				<xsl:for-each select="$distinctDays">
					<xsl:sort select="."/>
					<xsl:variable name="currentDay" as="xs:date" select="."/>
					<xsl:variable name="daysWorks" select="$workActivities[rb:date-from-dateTime(StartTime) = $currentDay]"/>
					<h2><xsl:value-of select="."/>: <xsl:value-of select="format-number(rb:sum-of-hours($daysWorks),'0.0')"/> h </h2>
					<div style="margin-left: 10mm;">
						<xsl:variable name="projects" select="distinct-values($daysWorks/Project)"/>
						<xsl:for-each select="$projects">
							<xsl:sort select="."/>
							<xsl:variable name="currentProject" select="."/>
							<xsl:variable name="currentProjectActs" select="$daysWorks[Project = $currentProject]"/>								
							<h3><xsl:value-of select="$currentProject"/>: <xsl:value-of select="format-number(rb:sum-of-hours($currentProjectActs),'0.0')"/> h </h3>
							<div style="margin-left: 10mm;">
								<xsl:variable name="descriptions" select="distinct-values($currentProjectActs/Description)"/>
								<xsl:for-each select="$descriptions">
									<xsl:variable name="currentDesc" select="."/> 
									<p>
										<xsl:value-of select="$currentDesc"/>:
										<xsl:value-of select="format-number(rb:sum-of-hours($currentProjectActs[Description = $currentDesc]),'0.0')"/>										
									</p>
								</xsl:for-each>
								&#160;
							</div>
						</xsl:for-each>
						&#160;
					</div>
				</xsl:for-each>
				<hr/>
				<h1>TOTAL: <xsl:value-of select="rb:sum-of-hours($workActivities)"/> h </h1>
			</body>
		</html>
	</xsl:template>
		
</xsl:transform>
