clear variables;
clc;
clear;
Data = xml_read('data.xml');
%%
DataCont = Data.SerilizableDataItem();
%%

Price = [DataCont.price];
Owners = [DataCont.owners];
Ranks = [DataCont.rank];
Tags = [DataCont.tags];
SortPrice = sort(Price);
SortPriceCull = [SortPrice(1,303:end)];

SortRank = sort(Ranks);
SortRankCull = [SortRank(1,1278:end)];

%SortRankCull
%%
MeanRank = mean(SortRankCull)
MedianRank = median(SortRankCull)
STDRank = std(SortRankCull)


MeanOwner = mean(Owners)
MedianOwner = median(Owners)
STDOwner = std(Owners)

MeanPrice = mean(SortPriceCull)
MedianPrice = median(SortPriceCull)
STDPrice = std(SortPriceCull)
%%
AllTags = [Tags.string];
CategorizeTags = categorical(AllTags);
CountTags = countcats(CategorizeTags);
[C, neworder] = sort(CountTags);

SortedCat = reordercats(CategorizeTags,neworder);
summary(SortedCat);

figure;

hist2 = histogram(SortedCat);
title('Tags Histogram')
[n,x] = hist(SortedCat);

barstring = num2str(n')
m = 1:335;
text(m,n,barstring, 'HorizontalAlignment','center', 'VerticalAlignment','bottom')

